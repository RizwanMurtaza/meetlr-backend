using Meetlr.Application.Common.Interfaces;
using Meetlr.Module.Billing.Application.Interfaces;
using Meetlr.Module.Billing.Domain.Entities;
using Meetlr.Module.Billing.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Meetlr.Module.Billing.Infrastructure.Services;

/// <summary>
/// Service for managing user credits and billing operations
/// </summary>
public class CreditService : ICreditService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreditService> _logger;

    // Default credit costs (fallback if not in database)
    private static readonly Dictionary<ServiceType, int> DefaultCreditCosts = new()
    {
        { ServiceType.Email, 1 },
        { ServiceType.WhatsApp, 5 },
        { ServiceType.SMS, 6 }
    };

    public CreditService(
        IUnitOfWork unitOfWork,
        ILogger<CreditService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    #region Balance Operations

    public async Task<int> GetBalanceAsync(Guid userId, CancellationToken ct = default)
    {
        var userCredits = await GetUserCreditsAsync(userId, ct);
        return userCredits?.Balance ?? 0;
    }

    public async Task<UserCredits> GetOrCreateUserCreditsAsync(Guid userId, CancellationToken ct = default)
    {
        var userCredits = await GetUserCreditsAsync(userId, ct);

        if (userCredits == null)
        {
            userCredits = new UserCredits
            {
                UserId = userId,
                Balance = 0,
                IsUnlimited = false,
                Version = 1
            };

            _unitOfWork.Repository<UserCredits>().Add(userCredits);
            await _unitOfWork.SaveChangesAsync(ct);

            _logger.LogInformation("Created credit record for user {UserId}", userId);
        }

        return userCredits;
    }

    public async Task<bool> HasSufficientCreditsAsync(Guid userId, ServiceType serviceType, CancellationToken ct = default)
    {
        var userCredits = await GetUserCreditsAsync(userId, ct);

        if (userCredits == null)
            return false;

        // Check unlimited first
        if (userCredits.IsUnlimited)
        {
            if (!userCredits.UnlimitedExpiresAt.HasValue || userCredits.UnlimitedExpiresAt.Value > DateTime.UtcNow)
                return true;
        }

        var cost = await GetServiceCreditCostAsync(serviceType, ct);
        return userCredits.Balance >= cost;
    }

    public async Task<bool> IsUnlimitedAsync(Guid userId, CancellationToken ct = default)
    {
        var userCredits = await GetUserCreditsAsync(userId, ct);

        if (userCredits == null || !userCredits.IsUnlimited)
            return false;

        // Check if unlimited has expired
        if (userCredits.UnlimitedExpiresAt.HasValue && userCredits.UnlimitedExpiresAt.Value <= DateTime.UtcNow)
            return false;

        return true;
    }

    #endregion

    #region Credit Operations

    public async Task<(bool Success, bool AlreadyCharged, bool AlreadySent)> ReserveCreditsAsync(
        Guid userId,
        ServiceType serviceType,
        Guid relatedEntityId,
        string? relatedEntityType = null,
        string? recipient = null,
        CancellationToken ct = default)
    {
        // Check if credits were already reserved for this notification (idempotency)
        var existingUsage = await _unitOfWork.Repository<UserUsageHistory>()
            .GetQueryable()
            .FirstOrDefaultAsync(u =>
                u.RelatedEntityId == relatedEntityId &&
                u.RelatedEntityType == relatedEntityType &&
                !u.IsDeleted &&
                u.Status != UsageStatus.Refunded, ct);

        if (existingUsage != null)
        {
            // Check if already confirmed (notification was successfully sent)
            var alreadySent = existingUsage.Status == UsageStatus.Confirmed;

            _logger.LogInformation(
                "Credits already reserved for {EntityType} {EntityId}, status: {Status}, alreadySent: {AlreadySent}",
                relatedEntityType, relatedEntityId, existingUsage.Status, alreadySent);

            // Return success=true (don't charge again), alreadyCharged=true, alreadySent based on status
            return (true, true, alreadySent);
        }

        var userCredits = await GetOrCreateUserCreditsAsync(userId, ct);
        var cost = await GetServiceCreditCostAsync(serviceType, ct);
        var isUnlimited = await IsUnlimitedAsync(userId, ct);

        // Check if user can afford the deduction
        if (!isUnlimited && userCredits.Balance < cost)
        {
            _logger.LogWarning(
                "Insufficient credits for user {UserId}. Balance: {Balance}, Required: {Cost}",
                userId, userCredits.Balance, cost);
            return (false, false, false); // Failed, not already charged, not already sent
        }

        // Deduct credits (only if not unlimited)
        if (!isUnlimited)
        {
            userCredits.DeductCredits(cost);
        }

        // Log usage history with Reserved status
        var usageHistory = new UserUsageHistory
        {
            UserId = userId,
            ServiceType = serviceType,
            CreditsUsed = cost,
            RelatedEntityId = relatedEntityId,
            RelatedEntityType = relatedEntityType,
            Recipient = recipient,
            BalanceAfter = userCredits.Balance,
            UsedAt = DateTime.UtcNow,
            WasUnlimited = isUnlimited,
            Status = UsageStatus.Reserved
        };

        _unitOfWork.Repository<UserUsageHistory>().Add(usageHistory);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Reserved {Cost} credits for {ServiceType} from user {UserId}. New balance: {Balance}. Unlimited: {IsUnlimited}",
            cost, serviceType, userId, userCredits.Balance, isUnlimited);

        return (true, false, false); // Success, not already charged, not already sent
    }

    public async Task ConfirmCreditsUsedAsync(Guid relatedEntityId, CancellationToken ct = default)
    {
        var usageHistory = await _unitOfWork.Repository<UserUsageHistory>()
            .GetQueryable()
            .FirstOrDefaultAsync(u =>
                u.RelatedEntityId == relatedEntityId &&
                u.Status == UsageStatus.Reserved &&
                !u.IsDeleted, ct);

        if (usageHistory == null)
        {
            _logger.LogWarning("No reserved usage found for entity {EntityId}", relatedEntityId);
            return;
        }

        usageHistory.Status = UsageStatus.Confirmed;
        _unitOfWork.Repository<UserUsageHistory>().Update(usageHistory);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Confirmed credit usage for entity {EntityId}, user {UserId}",
            relatedEntityId, usageHistory.UserId);
    }

    public async Task RefundCreditsAsync(Guid relatedEntityId, CancellationToken ct = default)
    {
        var usageHistory = await _unitOfWork.Repository<UserUsageHistory>()
            .GetQueryable()
            .FirstOrDefaultAsync(u =>
                u.RelatedEntityId == relatedEntityId &&
                u.Status == UsageStatus.Reserved &&
                !u.IsDeleted, ct);

        if (usageHistory == null)
        {
            _logger.LogWarning("No reserved usage found for entity {EntityId} to refund", relatedEntityId);
            return;
        }

        // Refund credits (only if not unlimited)
        if (!usageHistory.WasUnlimited)
        {
            var userCredits = await GetUserCreditsAsync(usageHistory.UserId, ct);
            if (userCredits != null)
            {
                userCredits.AddCredits(usageHistory.CreditsUsed);
                _logger.LogInformation(
                    "Refunded {Credits} credits to user {UserId}. New balance: {Balance}",
                    usageHistory.CreditsUsed, usageHistory.UserId, userCredits.Balance);
            }
        }

        usageHistory.Status = UsageStatus.Refunded;
        _unitOfWork.Repository<UserUsageHistory>().Update(usageHistory);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Refunded credit usage for entity {EntityId}, user {UserId}",
            relatedEntityId, usageHistory.UserId);
    }

    [Obsolete("Use ReserveCreditsAsync for notifications to prevent double charging")]
    public async Task<bool> DeductCreditsAsync(
        Guid userId,
        ServiceType serviceType,
        Guid? relatedEntityId = null,
        string? relatedEntityType = null,
        string? recipient = null,
        CancellationToken ct = default)
    {
        var userCredits = await GetOrCreateUserCreditsAsync(userId, ct);
        var cost = await GetServiceCreditCostAsync(serviceType, ct);
        var isUnlimited = await IsUnlimitedAsync(userId, ct);

        // Check if user can afford the deduction
        if (!isUnlimited && userCredits.Balance < cost)
        {
            _logger.LogWarning(
                "Insufficient credits for user {UserId}. Balance: {Balance}, Required: {Cost}",
                userId, userCredits.Balance, cost);
            return false;
        }

        // Deduct credits (only if not unlimited)
        if (!isUnlimited)
        {
            userCredits.DeductCredits(cost);
        }

        // Log usage history (always, even for unlimited)
        var usageHistory = new UserUsageHistory
        {
            UserId = userId,
            ServiceType = serviceType,
            CreditsUsed = cost,
            RelatedEntityId = relatedEntityId,
            RelatedEntityType = relatedEntityType,
            Recipient = recipient,
            BalanceAfter = userCredits.Balance,
            UsedAt = DateTime.UtcNow,
            WasUnlimited = isUnlimited,
            Status = UsageStatus.Confirmed
        };

        _unitOfWork.Repository<UserUsageHistory>().Add(usageHistory);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Deducted {Cost} credits for {ServiceType} from user {UserId}. New balance: {Balance}. Unlimited: {IsUnlimited}",
            cost, serviceType, userId, userCredits.Balance, isUnlimited);

        return true;
    }

    public async Task AddCreditsAsync(
        Guid userId,
        int amount,
        TransactionType type,
        string? description = null,
        decimal amountPaid = 0,
        string? stripePaymentIntentId = null,
        Guid? userPackageId = null,
        CancellationToken ct = default)
    {
        var userCredits = await GetOrCreateUserCreditsAsync(userId, ct);

        userCredits.AddCredits(amount);

        // Log topup history
        var topupHistory = new UserTopupHistory
        {
            UserId = userId,
            CreditsAdded = amount,
            AmountPaid = amountPaid,
            TransactionType = type,
            Description = description,
            StripePaymentIntentId = stripePaymentIntentId,
            UserPackageId = userPackageId,
            BalanceAfter = userCredits.Balance
        };

        _unitOfWork.Repository<UserTopupHistory>().Add(topupHistory);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Added {Amount} credits to user {UserId} via {Type}. New balance: {Balance}",
            amount, userId, type, userCredits.Balance);
    }

    public async Task TopupCreditsAsync(
        Guid userId,
        int amount,
        decimal amountPaid,
        string? stripePaymentIntentId = null,
        CancellationToken ct = default)
    {
        await AddCreditsAsync(
            userId,
            amount,
            TransactionType.Topup,
            $"One-off credit top-up of {amount} credits",
            amountPaid,
            stripePaymentIntentId,
            userPackageId: null,
            ct);

        _logger.LogInformation(
            "User {UserId} topped up {Amount} credits for ${AmountPaid}",
            userId, amount, amountPaid);
    }

    #endregion

    #region Credit Cost Lookup

    public async Task<int> GetServiceCreditCostAsync(ServiceType serviceType, CancellationToken ct = default)
    {
        var creditCost = await _unitOfWork.Repository<SystemCreditCost>()
            .GetQueryable()
            .FirstOrDefaultAsync(c => c.ServiceType == serviceType && c.IsActive && !c.IsDeleted, ct);

        if (creditCost != null)
            return creditCost.CreditCost;

        // Fallback to defaults if not in database
        return DefaultCreditCosts.TryGetValue(serviceType, out var defaultCost) ? defaultCost : 1;
    }

    #endregion

    #region Package Operations

    public async Task<UserPackage> SubscribeToPackageAsync(
        Guid userId,
        Guid packageId,
        string? stripeSubscriptionId = null,
        CancellationToken ct = default)
    {
        // Get the package
        var package = await _unitOfWork.Repository<Package>()
            .GetQueryable()
            .FirstOrDefaultAsync(p => p.Id == packageId && p.IsActive && !p.IsDeleted, ct)
            ?? throw new InvalidOperationException($"Package {packageId} not found or not active");

        // Cancel any existing active subscription
        var existingSubscription = await GetActivePackageAsync(userId, ct);
        if (existingSubscription != null)
        {
            existingSubscription.Cancel("Switched to new package");
        }

        // Create new subscription
        var userPackage = new UserPackage
        {
            UserId = userId,
            PackageId = packageId,
            Status = PackageStatus.Active,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(package.DurationDays),
            CreditsGranted = package.CreditsIncluded ?? 0,
            CreditsUsed = 0,
            StripeSubscriptionId = stripeSubscriptionId
        };

        _unitOfWork.Repository<UserPackage>().Add(userPackage);

        // Grant credits if package includes them
        if (package.CreditsIncluded.HasValue && package.CreditsIncluded.Value > 0)
        {
            await AddCreditsAsync(
                userId,
                package.CreditsIncluded.Value,
                TransactionType.PackageGrant,
                $"Credits from {package.Name} subscription",
                package.Price,
                stripeSubscriptionId,
                userPackage.Id,
                ct);
        }

        // Set unlimited flag if applicable
        if (package.IsUnlimited)
        {
            var userCredits = await GetOrCreateUserCreditsAsync(userId, ct);
            userCredits.IsUnlimited = true;
            userCredits.UnlimitedExpiresAt = userPackage.EndDate;
        }

        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "User {UserId} subscribed to package {PackageName} until {EndDate}",
            userId, package.Name, userPackage.EndDate);

        return userPackage;
    }

    public async Task<bool> CancelSubscriptionAsync(Guid userId, string? reason = null, CancellationToken ct = default)
    {
        var activePackage = await GetActivePackageAsync(userId, ct);

        if (activePackage == null)
        {
            _logger.LogWarning("No active subscription found for user {UserId}", userId);
            return false;
        }

        activePackage.Cancel(reason);

        // Remove unlimited flag if it was an unlimited package
        var userCredits = await GetUserCreditsAsync(userId, ct);
        if (userCredits != null && userCredits.IsUnlimited)
        {
            userCredits.IsUnlimited = false;
            userCredits.UnlimitedExpiresAt = null;
        }

        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Cancelled subscription for user {UserId}. Reason: {Reason}",
            userId, reason ?? "No reason provided");

        return true;
    }

    public async Task<UserPackage?> GetActivePackageAsync(Guid userId, CancellationToken ct = default)
    {
        return await _unitOfWork.Repository<UserPackage>()
            .GetQueryable()
            .Include(p => p.Package)
            .FirstOrDefaultAsync(
                p => p.UserId == userId &&
                     p.Status == PackageStatus.Active &&
                     p.EndDate > DateTime.UtcNow &&
                     !p.IsDeleted,
                ct);
    }

    public async Task<List<Package>> GetAvailablePackagesAsync(CancellationToken ct = default)
    {
        return await _unitOfWork.Repository<Package>()
            .GetQueryable()
            .Where(p => p.IsActive && !p.IsDeleted)
            .OrderBy(p => p.SortOrder)
            .ToListAsync(ct);
    }

    public async Task<UserBillingSummary> GetUserBillingSummaryAsync(Guid userId, CancellationToken ct = default)
    {
        var userCredits = await GetOrCreateUserCreditsAsync(userId, ct);
        var activePackage = await GetActivePackageAsync(userId, ct);

        // Get this month's usage
        var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var totalUsedThisMonth = await _unitOfWork.Repository<UserUsageHistory>()
            .GetQueryable()
            .Where(u => u.UserId == userId && u.UsedAt >= startOfMonth && !u.IsDeleted)
            .SumAsync(u => u.CreditsUsed, ct);

        // Get this month's top-ups
        var totalTopupsThisMonth = await _unitOfWork.Repository<UserTopupHistory>()
            .GetQueryable()
            .Where(t => t.UserId == userId &&
                        t.TransactionType == TransactionType.Topup &&
                        t.CreatedAt >= startOfMonth &&
                        !t.IsDeleted)
            .SumAsync(t => t.CreditsAdded, ct);

        return new UserBillingSummary
        {
            CurrentBalance = userCredits.Balance,
            IsUnlimited = userCredits.IsUnlimited,
            UnlimitedExpiresAt = userCredits.UnlimitedExpiresAt,
            ActivePackage = activePackage,
            TotalCreditsUsedThisMonth = totalUsedThisMonth,
            TotalTopupsThisMonth = totalTopupsThisMonth
        };
    }

    public async Task AssignFreePackageAsync(Guid userId, CancellationToken ct = default)
    {
        // Check if user already has any package (prevent duplicate assignment)
        var existingPackage = await _unitOfWork.Repository<UserPackage>()
            .GetQueryable()
            .AnyAsync(p => p.UserId == userId && !p.IsDeleted, ct);

        if (existingPackage)
        {
            _logger.LogInformation("User {UserId} already has a package, skipping free package assignment", userId);
            return;
        }

        // Find the free package (Type = PayAsYouGo, Price = 0, has credits)
        var freePackage = await _unitOfWork.Repository<Package>()
            .GetQueryable()
            .FirstOrDefaultAsync(p =>
                p.Type == PackageType.PayAsYouGo &&
                p.Price == 0 &&
                p.IsActive &&
                !p.IsDeleted, ct);

        if (freePackage == null)
        {
            _logger.LogWarning("Free package not found in database. User {UserId} will not receive free credits.", userId);
            return;
        }

        // Create user package subscription
        var userPackage = new UserPackage
        {
            UserId = userId,
            PackageId = freePackage.Id,
            Status = PackageStatus.Active,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(freePackage.DurationDays > 0 ? freePackage.DurationDays : 36500), // ~100 years if no duration
            CreditsGranted = freePackage.CreditsIncluded ?? 0,
            CreditsUsed = 0
        };

        _unitOfWork.Repository<UserPackage>().Add(userPackage);

        // Grant free credits
        if (freePackage.CreditsIncluded.HasValue && freePackage.CreditsIncluded.Value > 0)
        {
            var userCredits = await GetOrCreateUserCreditsAsync(userId, ct);
            userCredits.AddCredits(freePackage.CreditsIncluded.Value);

            // Log the grant
            var topupHistory = new UserTopupHistory
            {
                UserId = userId,
                CreditsAdded = freePackage.CreditsIncluded.Value,
                AmountPaid = 0,
                TransactionType = TransactionType.PackageGrant,
                Description = $"Welcome bonus - {freePackage.Name}",
                BalanceAfter = userCredits.Balance,
                UserPackageId = userPackage.Id
            };

            _unitOfWork.Repository<UserTopupHistory>().Add(topupHistory);
        }

        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Assigned free package '{PackageName}' with {Credits} credits to user {UserId}",
            freePackage.Name, freePackage.CreditsIncluded ?? 0, userId);
    }

    #endregion

    #region Private Helpers

    private async Task<UserCredits?> GetUserCreditsAsync(Guid userId, CancellationToken ct)
    {
        return await _unitOfWork.Repository<UserCredits>()
            .GetQueryable()
            .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsDeleted, ct);
    }

    #endregion
}
