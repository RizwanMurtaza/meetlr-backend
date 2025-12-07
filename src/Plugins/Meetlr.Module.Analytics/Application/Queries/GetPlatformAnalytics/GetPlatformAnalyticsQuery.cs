using MediatR;

namespace Meetlr.Module.Analytics.Application.Queries.GetPlatformAnalytics;

/// <summary>
/// Query to get platform-wide analytics (admin only)
/// </summary>
public record GetPlatformAnalyticsQuery : IRequest<GetPlatformAnalyticsQueryResponse>
{
    /// <summary>
    /// Tenant ID (for multi-tenant filtering, null for super admin)
    /// </summary>
    public Guid? TenantId { get; init; }

    /// <summary>
    /// Period filter: "7d", "30d", "90d", or "all"
    /// </summary>
    public string Period { get; init; } = "30d";
}
