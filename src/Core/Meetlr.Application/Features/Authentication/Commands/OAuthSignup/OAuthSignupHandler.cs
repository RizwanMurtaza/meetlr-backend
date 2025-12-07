using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Features.Authentication.Common;
using Meetlr.Application.Interfaces;
using Meetlr.Application.Plugins.Services;
using Meetlr.Domain.Entities.Tenancy;
using Meetlr.Domain.Entities.Users;
using Meetlr.Domain.Enums;
using Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Application.Features.Authentication.Commands.OAuthSignup;

/// <summary>
/// Handler for OAuth-based signup with automatic tenant creation
/// When a user signs up via Google/Outlook, we create:
/// 1. A tenant based on their username (e.g., username.mywebsite.com)
/// 2. A user account linked to that tenant
/// 3. An admin group with the user as admin
/// </summary>
public class OAuthSignupHandler : IRequestHandler<OAuthSignupCommand, OAuthSignupResponse>
{
    private readonly IIdentityService _identityService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;
    private readonly ICalendarService? _calendarService;
    private readonly IPluginAutoInstallService? _pluginAutoInstallService;
    private readonly IUserBillingService? _userBillingService;
    private readonly IUserEmailConfigurationService? _userEmailConfigurationService;

    public OAuthSignupHandler(
        IIdentityService identityService,
        IUnitOfWork unitOfWork,
        IAuditService auditService,
        ICalendarService? calendarService = null,
        IPluginAutoInstallService? pluginAutoInstallService = null,
        IUserBillingService? userBillingService = null,
        IUserEmailConfigurationService? userEmailConfigurationService = null)
    {
        _identityService = identityService;
        _unitOfWork = unitOfWork;
        _auditService = auditService;
        _calendarService = calendarService;
        _pluginAutoInstallService = pluginAutoInstallService;
        _userBillingService = userBillingService;
        _userEmailConfigurationService = userEmailConfigurationService;
    }

    public async Task<OAuthSignupResponse> Handle(
        OAuthSignupCommand request,
        CancellationToken cancellationToken)
    {
        bool isNewUser = false;
        bool isNewTenant = false;

        // Check if user already exists by email or OAuth provider ID
        var existingUser = await _unitOfWork.Repository<User>().GetQueryable()
            .Include(u => u.Tenant)
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        // Check if user is admin

        if (existingUser != null)
        {
            var isAdmin = await _unitOfWork.Repository<UserGroup>()
                .GetQueryable()
                .AnyAsync(ug => ug.UserId == existingUser.Id && ug.Group.IsAdminGroup, cancellationToken);
            // User exists - just generate token and return
            var token = await _identityService.GenerateTokenAsync(existingUser.Id, existingUser.Email ?? string.Empty, isAdmin, cancellationToken);
            var tokenExpiry = await _identityService.GetTokenExpiryAsync();

            // Generate refresh token
            var refreshToken = await _identityService.GenerateRefreshTokenAsync(
                existingUser.Id, request.IpAddress, request.DeviceInfo, cancellationToken);
            var refreshTokenExpiry = _identityService.GetRefreshTokenExpiry();

            // Update calendar integration via ICalendarService (linked to default schedule)
            if (_calendarService != null)
            {
                // Find user's default schedule to link calendar
                var existingUserDefaultSchedule = await _unitOfWork.Repository<Domain.Entities.Scheduling.AvailabilitySchedule>()
                    .GetQueryable()
                    .Where(s => s.UserId == existingUser.Id && s.IsDefault)
                    .FirstOrDefaultAsync(cancellationToken);

                if (existingUserDefaultSchedule != null)
                {
                    await _calendarService.ConnectCalendarToScheduleAsync(
                        existingUserDefaultSchedule.Id,
                        request.Provider,
                        request.Email,
                        request.AccessToken,
                        request.OAuthRefreshToken,
                        request.TokenExpiry,
                        cancellationToken);
                }
            }

            // Auto-install meeting type plugins based on OAuth provider (e.g., Google -> Google Meet)
            // Pass OAuth tokens so the plugin is automatically connected with the same account
            if (_pluginAutoInstallService != null)
            {
                await _pluginAutoInstallService.AutoInstallPluginsForOAuthProviderAsync(
                    existingUser.Id,
                    request.Provider,
                    request.AccessToken,
                    request.OAuthRefreshToken,
                    request.TokenExpiry,
                    request.Email,
                    cancellationToken);
            }

            return new OAuthSignupResponse
            {
                UserId = existingUser.Id,
                TenantId = existingUser.TenantId,
                Email = existingUser.Email ?? string.Empty,
                FirstName = existingUser.FirstName ?? string.Empty,
                LastName = existingUser.LastName ?? string.Empty,
                MeetlrUsername = existingUser.MeetlrUsername ?? string.Empty,
                Subdomain = existingUser.Tenant?.Subdomain ?? string.Empty,
                TenantUrl = $"https://{existingUser.Tenant?.Subdomain}.mywebsite.com",
                BookingUrl = $"https://{existingUser.Tenant?.Subdomain}.mywebsite.com/book/{existingUser.MeetlrUsername}",
                IsNewUser = false,
                IsNewTenant = false,
                JwtToken = token,
                TokenExpiry = tokenExpiry,
                RefreshToken = refreshToken,
                RefreshTokenExpiry = refreshTokenExpiry
            };
        }

        // New user - create tenant and user
        isNewUser = true;
        isNewTenant = true;

        // Step 1: Create tenant and admin group using helper
        var (tenant, adminGroup, finalUsername) = await TenantCreationHelper.CreateTenantWithAdminAsync(
            request.FirstName,
            request.LastName,
            request.Email,
            request.TimeZone,
            request.MeetlrUsername,
            _unitOfWork,
            cancellationToken);

        // Step 2: Create Identity User first (this will create the User entity in the database)
        var userId = Guid.NewGuid();
        var (succeeded, errors, identityUserId) = await _identityService.CreateOAuthUserAsync(
            request.Email,
            request.Provider,
            request.ProviderId,
            tenant.Id,
            userId);

        if (!succeeded)
        {
            var errorMessage = string.Join(", ", errors);
            throw UserErrors.UpdateProfileFailed(errorMessage, "Failed to create OAuth user");
        }

        // Step 3: Update the User entity with additional properties
        var user = await _unitOfWork.Repository<User>().GetQueryable()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            throw UserErrors.UpdateProfileFailed("User was created but could not be found", "User creation failed");
        }

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.ProfileImageUrl = request.ProfileImageUrl;
        user.TimeZone = request.TimeZone;
        user.MeetlrUsername = finalUsername;

        // Ensure TenantId is set (in case Identity creation didn't set it properly)
        if (user.TenantId == Guid.Empty || user.TenantId != tenant.Id)
        {
            user.TenantId = tenant.Id;
        }

        _unitOfWork.Repository<User>().Update(user);

        // Step 4: Add User to Admin Group
        TenantCreationHelper.CreateUserGroupAssignment(user.Id, adminGroup.Id, _unitOfWork);

        // Step 5: Create Default Availability Schedule
        var defaultSchedule = UserSetupHelper.CreateDefaultAvailabilitySchedule(
            user.Id, tenant.Id, user.TimeZone, _unitOfWork);

        // Step 6: Create Default User Settings
        var defaultCurrency = UserSetupHelper.GetDefaultCurrencyForTimezone(user.TimeZone);
        UserSetupHelper.CreateDefaultUserSettings(
            user.Id, tenant.Id, defaultCurrency, user.TimeZone, _unitOfWork);

        // Step 7: Save all changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Step 7.1: Assign free package to new user (grants 1000 credits)
        if (_userBillingService != null)
        {
            await _userBillingService.AssignFreePackageAsync(user.Id, cancellationToken);
        }

        // Step 7.2: Create default email configuration for user (Oracle SMTP)
        if (_userEmailConfigurationService != null)
        {
            var userName = $"{user.FirstName} {user.LastName}".Trim();
            await _userEmailConfigurationService.CreateDefaultEmailConfigurationAsync(
                user.Id,
                tenant.Id,
                user.Email!,
                userName,
                cancellationToken);
        }

        // Step 8: Create Calendar Integration via ICalendarService (linked to default schedule)
        if (_calendarService != null)
        {
            await _calendarService.ConnectCalendarToScheduleAsync(
                defaultSchedule.Id,
                request.Provider,
                request.Email,
                request.AccessToken,
                request.OAuthRefreshToken,
                request.TokenExpiry,
                cancellationToken);
        }

        // Step 8.1: Auto-install meeting type plugins based on OAuth provider (e.g., Google -> Google Meet)
        // Pass OAuth tokens so the plugin is automatically connected with the same account
        if (_pluginAutoInstallService != null)
        {
            await _pluginAutoInstallService.AutoInstallPluginsForOAuthProviderAsync(
                user.Id,
                request.Provider,
                request.AccessToken,
                request.OAuthRefreshToken,
                request.TokenExpiry,
                request.Email,
                cancellationToken);
        }

        // Step 9: Log audit trails
        await _auditService.LogAsync(AuditEntityType.Tenant, tenant.Id.ToString(), AuditAction.CreateViaOAuth, null, tenant, cancellationToken);
        await _auditService.LogAsync(AuditEntityType.User, user.Id.ToString(), AuditAction.CreateViaOAuth, null, user, cancellationToken);
        await _auditService.LogAsync(AuditEntityType.Group, adminGroup.Id.ToString(), AuditAction.Create, null, adminGroup, cancellationToken);
        await _auditService.LogAsync(AuditEntityType.AvailabilitySchedule, defaultSchedule.Id.ToString(), AuditAction.Create, null, defaultSchedule, cancellationToken);

        // Step 10: Generate JWT token (new user is always admin of their tenant)
        var jwtToken = await _identityService.GenerateTokenAsync(identityUserId, user.Email ?? string.Empty, isAdmin: true, cancellationToken);
        var jwtTokenExpiry = await _identityService.GetTokenExpiryAsync();

        // Step 11: Generate refresh token
        var jwtRefreshToken = await _identityService.GenerateRefreshTokenAsync(
            identityUserId, request.IpAddress, request.DeviceInfo, cancellationToken);
        var jwtRefreshTokenExpiry = _identityService.GetRefreshTokenExpiry();

        // Return response
        return new OAuthSignupResponse
        {
            UserId = user.Id,
            TenantId = tenant.Id,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName ?? string.Empty,
            LastName = user.LastName ?? string.Empty,
            MeetlrUsername = user.MeetlrUsername ?? string.Empty,
            Subdomain = tenant.Subdomain,
            TenantUrl = $"https://{tenant.Subdomain}.mywebsite.com",
            BookingUrl = $"https://{tenant.Subdomain}.mywebsite.com/book/{user.MeetlrUsername}",
            IsNewUser = isNewUser,
            IsNewTenant = isNewTenant,
            JwtToken = jwtToken,
            TokenExpiry = jwtTokenExpiry,
            RefreshToken = jwtRefreshToken,
            RefreshTokenExpiry = jwtRefreshTokenExpiry
        };
    }
}
