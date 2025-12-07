using Meetlr.Domain.Enums;

namespace Meetlr.Application.Interfaces;

public interface IAuditService
{
    Task LogAsync(AuditEntityType entityType, string entityId, AuditAction action, object? oldValues, object? newValues, CancellationToken cancellationToken = default);
}
