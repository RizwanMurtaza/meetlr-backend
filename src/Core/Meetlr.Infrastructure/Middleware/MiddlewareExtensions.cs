using Microsoft.AspNetCore.Builder;

namespace Meetlr.Infrastructure.Middleware;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseTenantResolution(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<TenantResolutionMiddleware>();
    }
}
