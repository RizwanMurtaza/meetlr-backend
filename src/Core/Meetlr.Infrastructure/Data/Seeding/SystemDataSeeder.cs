using Meetlr.Domain.Entities.Tenancy;
using Meetlr.Domain.Entities.Users;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Interfaces;
using Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Meetlr.Infrastructure.Data.Seeding;

/// <summary>
/// Seeds production-critical system data (admin user, tenant, group)
/// Note: Email seeding is handled by EmailSeeder in Meetlr.Module.Notifications
/// </summary>
public class SystemDataSeeder
{
    private readonly UserManager<User> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SystemDataSeeder> _logger;
    private readonly IIdentityService _identityService;

    public SystemDataSeeder(
        UserManager<User> userManager,
        IUnitOfWork unitOfWork,
        ILogger<SystemDataSeeder> logger,
        IIdentityService identityService)
    {
        _userManager = userManager;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _identityService = identityService;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting system data seeding...");

        try
        {

            // Check if admin user already exists
            var existingAdmin = await _userManager.FindByEmailAsync("admin@calendly.com");
            if (existingAdmin != null)
            {
                _logger.LogInformation("Admin user already exists, skipping admin user seeding");
                return;
            }

            // Create default admin tenant
            var adminTenant = new Tenant
            {
                Id = Guid.NewGuid(),
                Name = "Admin Organization",
                Subdomain = "admin",
                MainText = "Administration Portal",
                Description = "System administration organization",
                Email = "admin@calendly.com",
                TimeZone = "UTC",
                Language = "en",
                IsActive = true,
                IsSuperTenant = true,
                SubscriptionPlan = "Enterprise",
                MaxUsers = 1000,
                MaxBookingsPerMonth = 100000,
                MaxServices = 1000,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _unitOfWork.Repository<Tenant>().Add(adminTenant);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Created admin tenant: {TenantId}", adminTenant.Id);

            // Create admin user (User entity with Identity in one step)
            var userId = Guid.NewGuid();
            var result = await _identityService.CreateUserWithTenantAsync(
                "admin@calendly.com",
                "Admin@123",
                adminTenant.Id,
                userId);

            if (!result.Succeeded)
            {
                _logger.LogError("Failed to create admin user: {Errors}", string.Join(", ", result.Errors));
                throw ConfigurationErrors.SeedingFailed("AdminUser", string.Join(", ", result.Errors));
            }

            _logger.LogInformation("Created admin user: {UserId}", result.UserId);

            // Update the user with business properties
            var adminUser = await _userManager.FindByIdAsync(userId.ToString());
            if (adminUser != null)
            {
                adminUser.FirstName = "System";
                adminUser.LastName = "Administrator";
                adminUser.TimeZone = "UTC";
                adminUser.MeetlrUsername = "admin";
                adminUser.WelcomeMessage = "Welcome to Calendly Administration";
                adminUser.Language = "en";
                adminUser.DateFormat = "MM/dd/yyyy";
                adminUser.TimeFormat = "12h";
                adminUser.UseBranding = true;

                await _userManager.UpdateAsync(adminUser);
                _logger.LogInformation("Updated admin user with business properties");
            }

            // Create admin group
            var adminGroup = new Group
            {
                Id = Guid.NewGuid(),
                Name = "Administrators",
                Description = "System administrators with full access",
                IsAdminGroup = true,
                IsActive = true,
                TenantId = adminTenant.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _unitOfWork.Repository<Group>().Add(adminGroup);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Created admin group: {GroupId}", adminGroup.Id);

            // Assign admin user to admin group
            var userGroup = new UserGroup
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                GroupId = adminGroup.Id,
                IsAdmin = true,
                JoinedAt = DateTime.UtcNow
            };

            _unitOfWork.Repository<UserGroup>().Add(userGroup);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Assigned admin user to admin group");

            // Note: Email seeding (SystemEmailConfiguration, EmailTemplate, EmailProviderConfiguration)
            // is now handled by EmailSeeder in Meetlr.Module.Notifications which runs via ISeeder mechanism

            _logger.LogInformation("System data seeding completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while seeding system data");
            throw;
        }
    }
}
