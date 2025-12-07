using System.Reflection;
using Meetlr.Domain.Entities.Users;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Interfaces;
using Meetlr.Domain.Common;
using Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;
using Meetlr.Infrastructure.Data.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>, IApplicationDbContext
{
    private readonly ICurrentUserService? _currentUserService;
    private readonly ITenantService? _tenantService;
    private readonly IEnumerable<IPluginDbConfiguration>? _pluginConfigurations;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentUserService currentUserService)
        : base(options)
    {
        _currentUserService = currentUserService;
    }

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentUserService currentUserService,
        ITenantService tenantService)
        : base(options)
    {
        _currentUserService = currentUserService;
        _tenantService = tenantService;
    }

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentUserService currentUserService,
        ITenantService tenantService,
        IEnumerable<IPluginDbConfiguration> pluginConfigurations)
        : base(options)
    {
        _currentUserService = currentUserService;
        _tenantService = tenantService;
        _pluginConfigurations = pluginConfigurations;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply tenant-based query filters to all ITenantScoped entities
        if (_tenantService != null && _tenantService.IsTenantResolved)
        {
            modelBuilder.ApplyTenantFilters(_tenantService);
        }

        // Apply all entity configurations from this assembly (Infrastructure)
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Apply entity configurations from registered plugin assemblies
        if (_pluginConfigurations != null)
        {
            foreach (var pluginConfig in _pluginConfigurations)
            {
                modelBuilder.ApplyConfigurationsFromAssembly(pluginConfig.ConfigurationAssembly);
            }
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Get current user ID (already parsed as Guid in CurrentUserService)
        var currentUserId = _currentUserService?.UserId;
        var currentUserIdString = currentUserId?.ToString();

        // Auto-inject TenantId for all ITenantScoped entities
        var tenantId = _tenantService?.TenantId;
        if (tenantId != null && tenantId != Guid.Empty)
        {
            foreach (var entry in ChangeTracker.Entries<ITenantScoped>())
            {
                if (entry.State == EntityState.Added)
                {
                    // Auto-inject TenantId on create
                    if (entry.Entity.TenantId == Guid.Empty)
                    {
                        entry.Entity.TenantId = tenantId.Value;
                    }
                }
                else if (entry.State == EntityState.Modified)
                {
                    // Prevent TenantId from being changed
                    var originalTenantId = entry.OriginalValues.GetValue<Guid>(nameof(ITenantScoped.TenantId));
                    if (entry.Entity.TenantId != originalTenantId)
                    {
                        throw TenantErrors.TenantIdCannotBeModified();
                    }
                }
            }
        }

        // Update audit fields for User entities (since they don't inherit BaseEntity anymore)
        foreach (var entry in ChangeTracker.Entries<User>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.CreatedBy = currentUserId;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedBy = currentUserId;
                    break;
            }
        }

        // Update audit fields for other BaseEntity entities
        foreach (var entry in ChangeTracker.Entries<Domain.Common.BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.CreatedBy = currentUserIdString;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedBy = currentUserIdString;
                    break;
            }
        }

        // Handle soft deletes for BaseAuditableEntity
        foreach (var entry in ChangeTracker.Entries<BaseAuditableEntity>())
        {
            if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entry.Entity.IsDeleted = true;
                entry.Entity.DeletedAt = DateTime.UtcNow;
                entry.Entity.DeletedBy = currentUserIdString;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
