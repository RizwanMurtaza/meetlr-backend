using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Features.Authentication.Common;
using Meetlr.Application.Features.Emails.Commands.SendVerificationEmail;
using Meetlr.Application.Interfaces;
using Meetlr.Application.Plugins.Services;
using Meetlr.Domain.Entities.Users;
using Meetlr.Domain.Enums;
using Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;

namespace Meetlr.Application.Features.Authentication.Commands.Register;

/// <summary>
/// Handler for user registration
/// Creates a new tenant, user, and admin group
/// Sends verification email with OTP (2FA optional)
/// </summary>
public class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterCommandResponse>
{
    private readonly IIdentityService _identityService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;
    private readonly IOtpService _otpService;
    private readonly IMediator _mediator;
    private readonly IUserBillingService? _userBillingService;
    private readonly IUserEmailConfigurationService? _userEmailConfigurationService;

    public RegisterCommandHandler(
        IIdentityService identityService,
        IUnitOfWork unitOfWork,
        IAuditService auditService,
        IOtpService otpService,
        IMediator mediator,
        IUserBillingService? userBillingService = null,
        IUserEmailConfigurationService? userEmailConfigurationService = null)
    {
        _identityService = identityService;
        _unitOfWork = unitOfWork;
        _auditService = auditService;
        _otpService = otpService;
        _mediator = mediator;
        _userBillingService = userBillingService;
        _userEmailConfigurationService = userEmailConfigurationService;
    }

    public async Task<RegisterCommandResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // Step 1: Create tenant and admin group using helper
        var (tenant, adminGroup, finalUsername) = await TenantCreationHelper.CreateTenantWithAdminAsync(
            request.FirstName,
            request.LastName,
            request.Email,
            request.TimeZone,
            request.MeetlrUsername,
            _unitOfWork,
            cancellationToken);

        // Step 2: Create Identity user (this also creates the domain User entity)
        var userId = Guid.NewGuid();
        var (succeeded, errors, identityUserId) = await _identityService.CreateUserWithTenantAsync(
            request.Email,
            request.Password,
            tenant.Id,
            userId);

        if (!succeeded)
        {
            var errorMessage = string.Join(", ", errors);
            throw UserErrors.UpdateProfileFailed(errorMessage, "Failed to create user");
        }

        // Step 3: Get the created user and update business properties
        var user = await _unitOfWork.Repository<User>()
            .GetByIdAsync(userId, cancellationToken);

        if (user == null)
        {
            throw UserErrors.UpdateProfileFailed("User creation failed", "User not found after creation");
        }

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.TimeZone = request.TimeZone;
        user.MeetlrUsername = finalUsername;

        // Ensure TenantId is set (in case Identity creation didn't set it properly)
        if (user.TenantId == Guid.Empty || user.TenantId != tenant.Id)
        {
            user.TenantId = tenant.Id;
        }

        _unitOfWork.Repository<User>().Update(user);

        // Step 4: Add user to admin group
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

        // Step 7.1: Assign free package to user (grants 1000 credits)
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

        // Step 8: Log audit
        await _auditService.LogAsync(
            AuditEntityType.Tenant,
            tenant.Id.ToString(),
            AuditAction.Create,
            null,
            tenant,
            cancellationToken);

        await _auditService.LogAsync(
            AuditEntityType.User,
            user.Id.ToString(),
            AuditAction.Create,
            null,
            user,
            cancellationToken);

        await _auditService.LogAsync(
            AuditEntityType.Group,
            adminGroup.Id.ToString(),
            AuditAction.Create,
            null,
            adminGroup,
            cancellationToken);

        await _auditService.LogAsync(
            AuditEntityType.AvailabilitySchedule,
            defaultSchedule.Id.ToString(),
            AuditAction.Create,
            null,
            defaultSchedule,
            cancellationToken);

        // Step 9: Generate OTP for email verification
        var otpCode = await _otpService.GenerateOtpAsync(
            user.Id,
            OtpPurpose.EmailVerification,
            cancellationToken);

        // Step 10: Send verification email using CQRS command
        try
        {
            await _mediator.Send(new SendVerificationEmailCommand
            {
                UserId = user.Id,
                Email = user.Email!,
                VerificationCode = otpCode
            }, cancellationToken);
        }
        catch (Exception)
        {
            // Email sending failed - log but don't block registration
            // User can request resend later
        }

        // Step 11: Return response (NO JWT token until email verified)
        return new RegisterCommandResponse
        {
            UserId = user.Id,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            MeetlrUsername = user.MeetlrUsername ?? string.Empty,
            TenantId = tenant.Id,
            Subdomain = tenant.Subdomain,
            JwtToken = string.Empty, // No token until verified
            TokenExpiry = DateTime.UtcNow // Placeholder
        };
    }
}
