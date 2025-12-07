namespace Meetlr.Application.Features.Tenants.Commands.UpdateBranding;

public record UpdateBrandingResponse
{
    public Guid TenantId { get; init; }
    public string TenantName { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public DateTime UpdatedAt { get; init; }
}
