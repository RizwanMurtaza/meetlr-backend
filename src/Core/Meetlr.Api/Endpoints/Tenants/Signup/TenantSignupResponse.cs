using Meetlr.Application.Features.Tenants.Commands.CreateTenantWithAdmin;

namespace Meetlr.Api.Endpoints.Tenants.Signup;

public record TenantSignupResponse
{
    public Guid TenantId { get; init; }
    public string TenantName { get; init; } = string.Empty;
    public string Subdomain { get; init; } = string.Empty;
    public string TenantUrl { get; init; } = string.Empty;
    public Guid AdminUserId { get; init; }
    public string AdminEmail { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;

    public static TenantSignupResponse FromCommandResponse(CreateTenantWithAdminResponse response, string baseUrl)
    {
        return new TenantSignupResponse
        {
            TenantId = response.TenantId,
            TenantName = response.TenantName,
            Subdomain = response.Subdomain,
            TenantUrl = $"https://{response.Subdomain}.{baseUrl}",
            AdminUserId = response.AdminUserId,
            AdminEmail = response.AdminEmail,
            Message = response.Message
        };
    }
}
