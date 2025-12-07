using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Meetlr.Application.Interfaces;
using Meetlr.Domain.Common;

namespace Meetlr.Infrastructure.Data.Extensions;

public static class ModelBuilderExtensions
{
    /// <summary>
    /// Applies tenant-based query filters to all entities implementing ITenantScoped.
    /// Entities with an IsShared property will have a special filter allowing shared resources.
    /// </summary>
    public static void ApplyTenantFilters(this ModelBuilder modelBuilder, ITenantService tenantService)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // Only process entities that implement ITenantScoped
            if (typeof(ITenantScoped).IsAssignableFrom(entityType.ClrType))
            {
                // Check if entity has IsShared property (like Contact)
                var hasIsShared = entityType.ClrType.GetProperty("IsShared",
                    BindingFlags.Public | BindingFlags.Instance) != null;

                var method = typeof(ModelBuilderExtensions)
                    .GetMethod(
                        hasIsShared ? nameof(SetSharedTenantFilter) : nameof(SetTenantFilter),
                        BindingFlags.NonPublic | BindingFlags.Static
                    )
                    ?.MakeGenericMethod(entityType.ClrType);

                method?.Invoke(null, new object[] { modelBuilder, tenantService });
            }
        }
    }

    /// <summary>
    /// Sets a standard tenant filter: WHERE TenantId = @currentTenantId
    /// If no tenant context is available, no filter is applied (allows signup/system operations)
    /// </summary>
    private static void SetTenantFilter<TEntity>(
        ModelBuilder modelBuilder,
        ITenantService tenantService)
        where TEntity : class, ITenantScoped
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(e =>
            tenantService.TenantId == null || tenantService.TenantId == Guid.Empty ||
            e.TenantId == tenantService.TenantId);
    }

    /// <summary>
    /// Sets a shared tenant filter: WHERE TenantId = @currentTenantId OR IsShared = true
    /// Used for entities that can be shared across tenants (like Contact)
    /// If no tenant context is available, no filter is applied (allows signup/system operations)
    /// </summary>
    private static void SetSharedTenantFilter<TEntity>(
        ModelBuilder modelBuilder,
        ITenantService tenantService)
        where TEntity : class, ITenantScoped
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(e =>
            tenantService.TenantId == null || tenantService.TenantId == Guid.Empty ||
            e.TenantId == tenantService.TenantId ||
            EF.Property<bool>(e, "IsShared") == true);
    }
}
