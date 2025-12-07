using Meetlr.Application.Common.Interfaces;
using Meetlr.Domain.Entities.Tenancy;
using Meetlr.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Application.Features.Authentication.Common;

/// <summary>
/// Shared helper for creating tenants, admin groups, and user-group assignments
/// Used by both Register and OAuth signup flows
/// </summary>
public static class TenantCreationHelper
{
    /// <summary>
    /// Creates a new tenant with admin group and assigns the user to it
    /// </summary>
    /// <param name="firstName">User's first name</param>
    /// <param name="lastName">User's last name</param>
    /// <param name="email">User's email</param>
    /// <param name="timeZone">User's timezone</param>
    /// <param name="username">Optional Meetlr username (will be generated if not provided)</param>
    /// <param name="unitOfWork">Unit of work for repository access</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Tuple containing the created tenant, admin group, and final username</returns>
    public static async Task<(Tenant tenant, Group adminGroup, string username)> CreateTenantWithAdminAsync(
        string firstName,
        string lastName,
        string email,
        string timeZone,
        string? username,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken = default)
    {
        var userRepo = unitOfWork.Repository<User>();
        var tenantRepo = unitOfWork.Repository<Tenant>();

        // Generate username if not provided
        var finalUsername = username ?? GenerateUsername(firstName, lastName);

        // Ensure username is unique
        finalUsername = await EnsureUniqueUsernameAsync(finalUsername, userRepo, cancellationToken);

        // Generate subdomain from username (same as username by default)
        var subdomain = finalUsername.ToLower();

        // Ensure subdomain is unique
        subdomain = await EnsureUniqueSubdomainAsync(subdomain, tenantRepo, cancellationToken);

        // Create the Tenant
        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = $"{firstName} {lastName}",
            Subdomain = subdomain,
            MainText = $"Book time with {firstName}",
            Description = $"Schedule appointments with {firstName} {lastName}",
            Email = email,
            TimeZone = timeZone,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        tenantRepo.Add(tenant);

        // Create Admin Group for the Tenant
        var adminGroup = new Group
        {
            Id = Guid.NewGuid(),
            Name = "Administrators",
            Description = "Default admin group with full permissions",
            TenantId = tenant.Id,
            IsAdminGroup = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        unitOfWork.Repository<Group>().Add(adminGroup);

        return (tenant, adminGroup, finalUsername);
    }

    /// <summary>
    /// Creates a user-group assignment making the user an admin of the group
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="groupId">Group ID</param>
    /// <param name="unitOfWork">Unit of work for repository access</param>
    /// <returns>The created UserGroup entity</returns>
    public static UserGroup CreateUserGroupAssignment(Guid userId, Guid groupId, IUnitOfWork unitOfWork)
    {
        var userGroup = new UserGroup
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            GroupId = groupId,
            IsAdmin = true,
            JoinedAt = DateTime.UtcNow
        };

        unitOfWork.Repository<UserGroup>().Add(userGroup);

        return userGroup;
    }

    private static string GenerateUsername(string firstName, string lastName)
    {
        var baseUsername = $"{firstName.ToLower()}-{lastName.ToLower()}";
        var username = baseUsername.Replace(" ", "-");
        // Remove special characters
        username = new string(username.Where(c => char.IsLetterOrDigit(c) || c == '-').ToArray());
        return username;
    }

    private static async Task<string> EnsureUniqueUsernameAsync(
        string baseUsername,
        IRepository<User> userRepo,
        CancellationToken cancellationToken)
    {
        var username = baseUsername.ToLower();
        var counter = 1;

        while (await userRepo.GetQueryable().AnyAsync(u => u.MeetlrUsername == username, cancellationToken))
        {
            username = $"{baseUsername}-{counter}";
            counter++;
        }

        return username;
    }

    private static async Task<string> EnsureUniqueSubdomainAsync(
        string baseSubdomain,
        IRepository<Tenant> tenantRepo,
        CancellationToken cancellationToken)
    {
        var subdomain = baseSubdomain.ToLower();
        var counter = 1;

        while (await tenantRepo.GetQueryable().AnyAsync(t => t.Subdomain == subdomain, cancellationToken))
        {
            subdomain = $"{baseSubdomain}-{counter}";
            counter++;
        }

        return subdomain;
    }
}
