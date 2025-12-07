using Meetlr.Application.Features.Tenants.Commands.CreateTenantWithAdmin;

namespace Meetlr.Api.Endpoints.Tenants.Signup;

public record TenantSignupRequest
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

    public static CreateTenantWithAdminCommand ToCommand(TenantSignupRequest request)
    {
        return new CreateTenantWithAdminCommand
        {
            TenantName = request.TenantName,
            Subdomain = request.Subdomain,
            CustomDomain = request.CustomDomain,
            MainText = request.MainText,
            Description = request.Description,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Password = request.Password,
            PhoneNumber = request.PhoneNumber,
            TimeZone = request.TimeZone,
            PrimaryColor = request.PrimaryColor,
            SecondaryColor = request.SecondaryColor,
            LogoUrl = request.LogoUrl
        };
    }
}
