using MediatR;

namespace Meetlr.Application.Features.Tenants.Commands.CreateTenantWithAdmin;

/// <summary>
/// Command for creating a new tenant with an admin user during signup
/// </summary>
public record CreateTenantWithAdminCommand : IRequest<CreateTenantWithAdminResponse>
{
    // Tenant Information
    public string TenantName { get; init; } = string.Empty;
    public string Subdomain { get; init; } = string.Empty;
    public string? CustomDomain { get; init; }
    public string MainText { get; init; } = string.Empty;
    public string? Description { get; init; }

    // Admin User Information
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string? PhoneNumber { get; init; }
    public string TimeZone { get; init; } = "UTC";

    // Optional Branding
    public string? PrimaryColor { get; init; }
    public string? SecondaryColor { get; init; }
    public string? LogoUrl { get; init; }
}
