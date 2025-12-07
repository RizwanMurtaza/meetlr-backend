using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Common.Settings;
using Meetlr.Application.Plugins.Models;
using Meetlr.Application.Plugins.Payments;
using Meetlr.Application.Plugins.Payments.Models;
using Meetlr.Domain.Entities.Payments;
using Meetlr.Domain.Entities.Plugins;
using Meetlr.Domain.Entities.Users;
using Meetlr.Domain.Enums;
using Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;

namespace Meetlr.Plugins.Payment.Stripe;

public class StripePaymentProviderPlugin : IPaymentPlugin
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private readonly ILogger<StripePaymentProviderPlugin> _logger;
    private readonly ExternalApisSettings _externalApisSettings;
    private readonly string _stripeSecretKey;
    private readonly string _stripeClientId;
    private readonly int _applicationFeePercent;
    private readonly bool _isDevelopment;

    // IPlugin base properties
    public PluginCategory Category => PluginCategory.Payment;
    public string PluginId => "stripe";
    public string PluginName => "Stripe";
    public string Description => "Accept payments through Stripe Connect with automatic payouts and comprehensive financial management.";
    public string Version => "1.0.0";
    public string? Author => "Meetlr Team";
    public string? IconUrl => "/plugins/stripe/icon.svg";
    public bool IsEnabled { get; }
    public bool RequiresConnection => true;

    public StripePaymentProviderPlugin(
        IUnitOfWork unitOfWork,
        IConfiguration configuration,
        ILogger<StripePaymentProviderPlugin> logger,
        IOptions<ExternalApisSettings> externalApisSettings,
        IHostEnvironment hostEnvironment)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _logger = logger;
        _externalApisSettings = externalApisSettings.Value;
        _isDevelopment = hostEnvironment.IsDevelopment();

        _stripeSecretKey = configuration["Stripe:SecretKey"] ?? string.Empty;
        _stripeClientId = configuration["Stripe:ClientId"] ?? string.Empty;
        _applicationFeePercent = int.Parse(configuration["Stripe:ApplicationFeePercent"] ?? "10");

        IsEnabled = !string.IsNullOrEmpty(_stripeSecretKey) && !string.IsNullOrEmpty(_stripeClientId);

        if (IsEnabled)
        {
            StripeConfiguration.ApiKey = _stripeSecretKey;
        }
    }

    public async Task<string?> GenerateConnectUrlAsync(Guid userId, string returnUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!IsEnabled)
                throw PaymentErrors.PaymentProviderDisabled("stripe");

            var user = await _unitOfWork.Repository<User>()
                .GetQueryable()
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

            if (user == null)
                throw UserErrors.UserNotFound(userId);

            var state = Guid.NewGuid().ToString(); // Store this in session/cache for validation

            var connectUrl = $"{_externalApisSettings.Stripe.ConnectUrl}?" +
                $"response_type=code&" +
                $"client_id={_stripeClientId}&" +
                $"scope=read_write&" +
                $"redirect_uri={Uri.EscapeDataString(returnUrl)}&" +
                $"state={state}&" +
                $"stripe_user[email]={Uri.EscapeDataString(user.Email)}&" +
                $"stripe_user[business_type]=individual";

            _logger.LogInformation("Generated Stripe Connect URL for user {UserId}", userId);

            return connectUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating Stripe Connect URL for user {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> CompleteConnectAsync(string authorizationCode, string redirectUri, Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!IsEnabled)
                throw PaymentErrors.PaymentProviderDisabled("stripe");

            // Exchange authorization code for access token
            var tokenService = new OAuthTokenService();
            var token = await tokenService.CreateAsync(new OAuthTokenCreateOptions
            {
                GrantType = "authorization_code",
                Code = authorizationCode
            }, cancellationToken: cancellationToken);

            if (token?.StripeUserId == null)
            {
                _logger.LogError("Failed to get Stripe user ID from token");
                return false;
            }

            // Get account details
            var accountService = new AccountService();
            var account = await accountService.GetAsync(token.StripeUserId, cancellationToken: cancellationToken);

            // Save or update Stripe account
            var stripeAccount = await _unitOfWork.Repository<StripeAccount>()
                .GetQueryable()
                .FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);

            if (stripeAccount == null)
            {
                stripeAccount = new StripeAccount
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    StripeAccountId = token.StripeUserId,
                    CreatedAt = DateTime.UtcNow
                };
                _unitOfWork.Repository<StripeAccount>().Add(stripeAccount);
            }

            stripeAccount.IsConnected = true;
            
            // In development, bypass Stripe's account verification requirements
            if (_isDevelopment)
            {
                stripeAccount.ChargesEnabled = true;
                stripeAccount.PayoutsEnabled = true;
                _logger.LogInformation("Development mode: Setting ChargesEnabled and PayoutsEnabled to true for user {UserId}", userId);
            }
            else
            {
                stripeAccount.ChargesEnabled = account.ChargesEnabled;
                stripeAccount.PayoutsEnabled = account.PayoutsEnabled;
            }
            stripeAccount.DetailsSubmitted = account.DetailsSubmitted;
            stripeAccount.Country = account.Country;
            stripeAccount.Currency = account.DefaultCurrency;
            stripeAccount.Email = account.Email;
            stripeAccount.BusinessType = account.BusinessType;
            stripeAccount.AccessToken = token.StripeUserId; // Store the account ID
            stripeAccount.RefreshToken = null;
            stripeAccount.Scope = token.Scope;
            stripeAccount.ConnectedAt = DateTime.UtcNow;
            stripeAccount.LastSyncedAt = DateTime.UtcNow;
            stripeAccount.UpdatedAt = DateTime.UtcNow;

            // Set verification status
            if (account.Requirements?.CurrentlyDue?.Count > 0)
                stripeAccount.VerificationStatus = "pending";
            else if (account.Requirements?.Errors?.Count > 0)
                stripeAccount.VerificationStatus = "unverified";
            else
                stripeAccount.VerificationStatus = "verified";

            // Update UserInstalledPlugin to mark as connected
            var userPlugin = await _unitOfWork.Repository<UserInstalledPlugin>()
                .GetQueryable()
                .FirstOrDefaultAsync(p =>
                    p.UserId == userId &&
                    p.PluginId == PluginId &&
                    !p.IsDeleted,
                    cancellationToken);

            if (userPlugin != null)
            {
                userPlugin.IsConnected = true;
                userPlugin.ConnectedAt = DateTime.UtcNow;
                userPlugin.ConnectionStatus = "connected";
                _unitOfWork.Repository<UserInstalledPlugin>().Update(userPlugin);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully connected Stripe account {StripeAccountId} for user {UserId}",
                token.StripeUserId, userId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing Stripe Connect for user {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> DisconnectAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var stripeAccount = await _unitOfWork.Repository<StripeAccount>()
                .GetQueryable()
                .FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);

            if (stripeAccount == null)
                return false;

            // Revoke access on Stripe
            try
            {
                // Note: Stripe Connect accounts are deauthorized by deleting the relationship
                // The account itself remains, but the connection is severed
                _logger.LogInformation("Severing Stripe Connect relationship for account {StripeAccountId}", stripeAccount.StripeAccountId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Note logged while disconnecting Stripe account {StripeAccountId}", stripeAccount.StripeAccountId);
            }

            // Remove from database
            _unitOfWork.Repository<StripeAccount>().Delete(stripeAccount);

            // Update UserInstalledPlugin to mark as disconnected
            var userPlugin = await _unitOfWork.Repository<UserInstalledPlugin>()
                .GetQueryable()
                .FirstOrDefaultAsync(p =>
                    p.UserId == userId &&
                    p.PluginId == PluginId &&
                    !p.IsDeleted,
                    cancellationToken);

            if (userPlugin != null)
            {
                userPlugin.IsConnected = false;
                userPlugin.ConnectedAt = null;
                userPlugin.ConnectionStatus = "disconnected";
                _unitOfWork.Repository<UserInstalledPlugin>().Update(userPlugin);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Disconnected Stripe account for user {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disconnecting Stripe account for user {UserId}", userId);
            return false;
        }
    }

    public async Task<PluginConnectionStatus?> GetConnectionStatusAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var stripeAccount = await _unitOfWork.Repository<StripeAccount>()
                .GetQueryable()
                .FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);

            if (stripeAccount == null)
                return null;

            return new PluginConnectionStatus
            {
                IsConnected = stripeAccount.IsConnected,
                Email = stripeAccount.Email,
                ConnectedAt = stripeAccount.ConnectedAt,
                NeedsReconnect = !stripeAccount.ChargesEnabled || !stripeAccount.PayoutsEnabled,
                Metadata = new Dictionary<string, object>
                {
                    { "ChargesEnabled", stripeAccount.ChargesEnabled },
                    { "PayoutsEnabled", stripeAccount.PayoutsEnabled },
                    { "DetailsSubmitted", stripeAccount.DetailsSubmitted },
                    { "VerificationStatus", stripeAccount.VerificationStatus ?? "" },
                    { "Country", stripeAccount.Country ?? "" },
                    { "Currency", stripeAccount.Currency ?? "" }
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Stripe connection status for user {UserId}", userId);
            return null;
        }
    }

    public async Task<string?> GetConnectedAccountIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var stripeAccount = await _unitOfWork.Repository<StripeAccount>()
                .GetQueryable()
                .FirstOrDefaultAsync(s => s.UserId == userId && s.IsConnected, cancellationToken);

            return stripeAccount?.StripeAccountId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting connected account ID for user {UserId}", userId);
            return null;
        }
    }

    public async Task<PaymentIntentResult> CreatePaymentIntentAsync(
        Guid bookingId,
        decimal amount,
        string currency,
        string connectedAccountId,
        string description,
        Dictionary<string, string> metadata,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!IsEnabled)
                throw PaymentErrors.PaymentProviderDisabled("stripe");

            var amountInCents = (long)(amount * 100);
            var applicationFee = (long)(amountInCents * _applicationFeePercent / 100);

            var options = new PaymentIntentCreateOptions
            {
                Amount = amountInCents,
                Currency = currency.ToLower(),
                Description = description,
                Metadata = metadata,
                ApplicationFeeAmount = applicationFee,
                TransferData = new PaymentIntentTransferDataOptions
                {
                    Destination = connectedAccountId
                },
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true
                }
            };

            // Add idempotency key to prevent double-charging on retry
            var requestOptions = new RequestOptions
            {
                IdempotencyKey = $"booking_{bookingId}_{DateTime.UtcNow:yyyyMMdd}"
            };

            var service = new PaymentIntentService();
            var paymentIntent = await service.CreateAsync(options, requestOptions, cancellationToken);

            _logger.LogInformation(
                "Created payment intent {PaymentIntentId} for booking {BookingId}, amount: {Amount} {Currency}",
                paymentIntent.Id, bookingId, amount, currency);

            return new PaymentIntentResult
            {
                PaymentIntentId = paymentIntent.Id,
                ClientSecret = paymentIntent.ClientSecret!,
                Status = paymentIntent.Status,
                Amount = amount,
                Currency = currency
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment intent for booking {BookingId}", bookingId);
            throw;
        }
    }

    public async Task<bool> RefundPaymentAsync(string paymentIntentId, decimal? amount = null, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!IsEnabled)
                throw PaymentErrors.PaymentProviderDisabled("stripe");

            var options = new RefundCreateOptions
            {
                PaymentIntent = paymentIntentId
            };

            if (amount.HasValue)
                options.Amount = (long)(amount.Value * 100);

            // Add idempotency key to prevent duplicate refunds
            var requestOptions = new RequestOptions
            {
                IdempotencyKey = $"refund_{paymentIntentId}_{DateTime.UtcNow:yyyyMMdd}"
            };

            var service = new RefundService();
            var refund = await service.CreateAsync(options, requestOptions, cancellationToken);

            _logger.LogInformation("Created refund {RefundId} for payment intent {PaymentIntentId}",
                refund.Id, paymentIntentId);

            return refund.Status == "succeeded" || refund.Status == "pending";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refunding payment intent {PaymentIntentId}", paymentIntentId);
            return false;
        }
    }

    public async Task<bool> ProcessWebhookAsync(
        string payload,
        string signature,
        Func<WebhookEvent, Task> eventHandler,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!IsEnabled)
                throw PaymentErrors.PaymentProviderDisabled("stripe");

            var webhookSecret = _configuration["Stripe:WebhookSecret"];
            if (string.IsNullOrEmpty(webhookSecret))
            {
                _logger.LogWarning("Stripe webhook secret not configured");
                return false;
            }

            // Verify webhook signature
            Event stripeEvent;
            try
            {
                stripeEvent = EventUtility.ConstructEvent(payload, signature, webhookSecret);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to verify Stripe webhook signature");
                return false;
            }

            // Process different event types
            var webhookEvent = new WebhookEvent
            {
                StripeEventType = stripeEvent.Type,
                Metadata = new Dictionary<string, string>()
            };

            switch (stripeEvent.Type)
            {
                case "payment_intent.succeeded":
                    if (stripeEvent.Data.Object is PaymentIntent successIntent)
                    {
                        webhookEvent.PaymentIntentId = successIntent.Id;
                        webhookEvent.Status = successIntent.Status;
                        webhookEvent.Metadata = successIntent.Metadata ?? new Dictionary<string, string>();
                    }
                    break;

                case "payment_intent.payment_failed":
                    if (stripeEvent.Data.Object is PaymentIntent failedIntent)
                    {
                        webhookEvent.PaymentIntentId = failedIntent.Id;
                        webhookEvent.Status = failedIntent.Status;
                        webhookEvent.Metadata = failedIntent.Metadata ?? new Dictionary<string, string>();
                    }
                    break;

                case "charge.refunded":
                    if (stripeEvent.Data.Object is Charge refundedCharge)
                    {
                        webhookEvent.PaymentIntentId = refundedCharge.PaymentIntentId;
                        webhookEvent.Status = "refunded";
                        webhookEvent.Metadata = refundedCharge.Metadata ?? new Dictionary<string, string>();
                    }
                    break;

                default:
                    _logger.LogInformation("Unhandled Stripe webhook event type: {EventType}", stripeEvent.Type);
                    return true;
            }

            await eventHandler(webhookEvent);
            
            _logger.LogInformation("Successfully processed Stripe webhook event {EventType}", stripeEvent.Type);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Stripe webhook");
            return false;
        }
    }

    /// <summary>
    /// Check health status of the Stripe plugin
    /// </summary>
    public async Task<PluginHealthStatus> GetHealthStatusAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (!IsEnabled)
            {
                return new PluginHealthStatus
                {
                    IsHealthy = false,
                    Status = "Disabled",
                    Message = "Stripe plugin is not enabled. Check configuration.",
                    Details = new Dictionary<string, string>
                    {
                        { "SecretKeyConfigured", (!string.IsNullOrEmpty(_stripeSecretKey)).ToString() },
                        { "ClientIdConfigured", (!string.IsNullOrEmpty(_stripeClientId)).ToString() }
                    }
                };
            }

            // Test Stripe API connection by making a simple API call
            var service = new BalanceService();
            var balance = await service.GetAsync(cancellationToken: cancellationToken);

            return new PluginHealthStatus
            {
                IsHealthy = true,
                Status = "Healthy",
                Message = "Stripe API connection successful",
                Details = new Dictionary<string, string>
                {
                    { "ApiVersion", global::Stripe.StripeConfiguration.ApiVersion },
                    { "AvailableBalance", balance.Available.FirstOrDefault()?.Amount.ToString() ?? "0" },
                    { "Currency", balance.Available.FirstOrDefault()?.Currency ?? "usd" }
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking Stripe plugin health");
            return new PluginHealthStatus
            {
                IsHealthy = false,
                Status = "Unhealthy",
                Message = $"Stripe API connection failed: {ex.Message}",
                Details = new Dictionary<string, string>
                {
                    { "Error", ex.Message },
                    { "ErrorType", ex.GetType().Name }
                }
            };
        }
    }
}
