using Bogus;
using  Meetlr.Domain.Entities.Tenancy;
using  Meetlr.Domain.Entities.Users;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Meetlr.Infrastructure.Data.Seeding;

/// <summary>
/// Seeds user data for testing
/// </summary>
public class UserSeeder : ISeeder
{
    private readonly UserManager<User> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIdentityService _identityService;
    private readonly ILogger<UserSeeder> _logger;

    public int Order => 3;

    public UserSeeder(
        UserManager<User> userManager,
        IUnitOfWork unitOfWork,
        IIdentityService identityService,
        ILogger<UserSeeder> logger)
    {
        _userManager = userManager;
        _unitOfWork = unitOfWork;
        _identityService = identityService;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting user seeding...");

        try
        {
            // Get test tenants (excluding admin tenant)
            var tenants = await _unitOfWork.Repository<Tenant>()
                .GetQueryable()
                .Where(t => !t.IsSuperTenant)
                .ToListAsync(cancellationToken);

            if (!tenants.Any())
            {
                _logger.LogWarning("No test tenants found, skipping user seeding");
                return;
            }

            // Check if test users already exist
            var existingTestUsers = await _userManager.FindByEmailAsync("test.user1@example.com");
            if (existingTestUsers != null)
            {
                _logger.LogInformation("Test users already exist, skipping seeding");
                return;
            }

            var faker = new Faker();
            var userCount = 0;

            foreach (var tenant in tenants)
            {
                // Check if admin group already exists for this tenant
                var existingAdminGroup = await _unitOfWork.Repository<Group>()
                    .GetQueryable()
                    .FirstOrDefaultAsync(g => g.TenantId == tenant.Id && g.Name == "Administrators", cancellationToken);

                Group adminGroup;
                if (existingAdminGroup != null)
                {
                    adminGroup = existingAdminGroup;
                    _logger.LogInformation("Admin group already exists for tenant: {TenantName}", tenant.Name);
                }
                else
                {
                    // Create admin group for this tenant
                    adminGroup = new Group
                    {
                        Id = Guid.NewGuid(),
                        Name = "Administrators",
                        Description = $"Administrators for {tenant.Name}",
                        IsAdminGroup = true,
                        IsActive = true,
                        TenantId = tenant.Id,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _unitOfWork.Repository<Group>().Add(adminGroup);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation("Created admin group for tenant: {TenantName}", tenant.Name);
                }

                // Create 2-3 users per tenant
                var usersToCreate = faker.Random.Int(2, 3);

                for (int i = 0; i < usersToCreate; i++)
                {
                    userCount++;
                    var firstName = faker.Name.FirstName();
                    var lastName = faker.Name.LastName();
                    var email = $"test.user{userCount}@example.com";
                    var username = $"{firstName.ToLower()}.{lastName.ToLower()}".Replace(" ", "");

                    // Create user (User entity with Identity in one step)
                    var userId = Guid.NewGuid();
                    var result = await _identityService.CreateUserWithTenantAsync(
                        email,
                        "User@123",
                        tenant.Id,
                        userId);

                    if (!result.Succeeded)
                    {
                        _logger.LogError("Failed to create user {Email}: {Errors}",
                            email, string.Join(", ", result.Errors));
                        continue;
                    }

                    // Update the user with business properties
                    var user = await _userManager.FindByIdAsync(userId.ToString());
                    if (user != null)
                    {
                        user.FirstName = firstName;
                        user.LastName = lastName;
                        user.PhoneNumber = faker.Phone.PhoneNumber();
                        user.Bio = faker.Lorem.Sentence(10);
                        user.CompanyName = tenant.Name;
                        user.TimeZone = tenant.TimeZone;
                        user.MeetlrUsername = username;
                        user.WelcomeMessage = $"Welcome! Book a time with {firstName}.";
                        user.Language = "en";
                        user.DateFormat = "MM/dd/yyyy";
                        user.TimeFormat = "12h";
                        user.UseBranding = true;
                        user.BrandColor = faker.Internet.Color();

                        await _userManager.UpdateAsync(user);
                    }

                    _logger.LogInformation("Created user: {Email} for tenant: {TenantName}",
                        email, tenant.Name);

                    // Assign first user of each tenant to admin group
                    if (i == 0)
                    {
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

                        _logger.LogInformation("Assigned user {Email} to admin group", email);
                    }
                }
            }

            _logger.LogInformation("Successfully seeded {Count} users across {TenantCount} tenants",
                userCount, tenants.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while seeding users");
            throw;
        }
    }
}
