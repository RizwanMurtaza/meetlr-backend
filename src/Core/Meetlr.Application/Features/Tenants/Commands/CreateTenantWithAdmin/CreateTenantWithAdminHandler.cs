using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Interfaces;
using Meetlr.Domain.Entities.Tenancy;
using Meetlr.Domain.Entities.Users;
using Meetlr.Domain.Enums;
using Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Application.Features.Tenants.Commands.CreateTenantWithAdmin;

/// <summary>
/// Handler for creating a new tenant with an admin user
/// </summary>
public class CreateTenantWithAdminHandler : IRequestHandler<CreateTenantWithAdminCommand, CreateTenantWithAdminResponse>
{
    private readonly IIdentityService _identityService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;

    public CreateTenantWithAdminHandler(
        IIdentityService identityService,
        IUnitOfWork unitOfWork,
        IAuditService auditService)
    {
        _identityService = identityService;
        _unitOfWork = unitOfWork;
        _auditService = auditService;
    }

    public async Task<CreateTenantWithAdminResponse> Handle(
        CreateTenantWithAdminCommand request,
        CancellationToken cancellationToken)
    {
        // Validate subdomain is unique
        var subdomainExists = await _unitOfWork.Repository<Tenant>().GetQueryable()
            .AnyAsync(t => t.Subdomain == request.Subdomain.ToLower(), cancellationToken);

        if (subdomainExists)
        {
            throw TenantErrors.SubdomainAlreadyExists(request.Subdomain);
        }

        // Validate custom domain is unique (if provided)
        if (!string.IsNullOrEmpty(request.CustomDomain))
        {
            var customDomainExists = await _unitOfWork.Repository<Tenant>().GetQueryable()
                .AnyAsync(t => t.CustomDomain == request.CustomDomain.ToLower(), cancellationToken);

            if (customDomainExists)
            {
                throw TenantErrors.CustomDomainAlreadyExists(request.CustomDomain);
            }
        }

        // Validate email is unique across all tenants
        var emailExists = await _unitOfWork.Repository<User>().GetQueryable()
            .AnyAsync(u => u.Email == request.Email, cancellationToken);

        if (emailExists)
        {
            throw UserErrors.UserAlreadyExists(request.Email);
        }

        // Step 1: Create the Tenant
        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = request.TenantName,
            Subdomain = request.Subdomain.ToLower(),
            CustomDomain = request.CustomDomain?.ToLower(),
            MainText = request.MainText,
            Description = request.Description,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            PrimaryColor = request.PrimaryColor ?? "#3B82F6",
            SecondaryColor = request.SecondaryColor ?? "#10B981",
            LogoUrl = request.LogoUrl,
            TimeZone = request.TimeZone,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _unitOfWork.Repository<Tenant>().Add(tenant);

        // Step 2: Create the Admin User (Domain Entity)
        var adminUser = new User
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            TimeZone = request.TimeZone,
            TenantId = tenant.Id,
            MeetlrUsername = GenerateUsername(request.FirstName, request.LastName),
            CreatedAt = DateTime.UtcNow
        };

        _unitOfWork.Repository<User>().Add(adminUser);

        // Step 3: Create Identity User (User)
        var (succeeded, errors, identityUserId) = await _identityService.CreateUserWithTenantAsync(
            request.Email,
            request.Password,
            tenant.Id,
            adminUser.Id);

        if (!succeeded)
        {
            var errorMessage = string.Join(", ", errors);
            throw UserErrors.UpdateProfileFailed(errorMessage, "Failed to create admin user");
        }

        // Step 4: Create Admin Group for the Tenant
        var adminGroup = new Group
        {
            Id = Guid.NewGuid(),
            Name = "Administrators",
            Description = "Default admin group for tenant with full permissions",
            TenantId = tenant.Id,
            IsAdminGroup = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _unitOfWork.Repository<Group>().Add(adminGroup);

        // Step 5: Add User to Admin Group
        var userGroup = new UserGroup
        {
            Id = Guid.NewGuid(),
            UserId = adminUser.Id,
            GroupId = adminGroup.Id,
            IsAdmin = true,
            JoinedAt = DateTime.UtcNow
        };

        _unitOfWork.Repository<UserGroup>().Add(userGroup);

        // Step 6: Save all changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Step 7: Log audit trails
        await _auditService.LogAsync(AuditEntityType.Tenant, tenant.Id.ToString(), AuditAction.Create, null, tenant, cancellationToken);
        await _auditService.LogAsync(AuditEntityType.User, adminUser.Id.ToString(), AuditAction.Create, null, adminUser, cancellationToken);
        await _auditService.LogAsync(AuditEntityType.Group, adminGroup.Id.ToString(), AuditAction.Create, null, adminGroup, cancellationToken);

        // Return response
        return new CreateTenantWithAdminResponse
        {
            TenantId = tenant.Id,
            TenantName = tenant.Name,
            Subdomain = tenant.Subdomain,
            AdminUserId = adminUser.Id,
            AdminGroupId = adminGroup.Id,
            AdminEmail = adminUser.Email,
            Message = $"Tenant '{tenant.Name}' created successfully with admin user. Access at: {tenant.Subdomain}.mywebsite.com"
        };
    }

    private static string GenerateUsername(string firstName, string lastName)
    {
        var baseUsername = $"{firstName.ToLower()}-{lastName.ToLower()}";
        var username = baseUsername.Replace(" ", "-");
        // Remove special characters
        username = new string(username.Where(c => char.IsLetterOrDigit(c) || c == '-').ToArray());
        return username;
    }
}
