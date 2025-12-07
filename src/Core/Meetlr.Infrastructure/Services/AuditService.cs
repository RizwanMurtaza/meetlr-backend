using System.Text.Json;
using  Meetlr.Domain.Entities.Auditing;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Interfaces;
using Meetlr.Domain.Enums;

namespace Meetlr.Infrastructure.Services;

public class AuditService : IAuditService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public AuditService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task LogAsync(AuditEntityType entityType, string entityId, AuditAction action, object? oldValues, object? newValues, CancellationToken cancellationToken = default)
    {
        var options = new JsonSerializerOptions
        {
            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles,
            MaxDepth = 32,
            WriteIndented = false
        };

        var auditLog = new AuditLog
        {
            UserId = _currentUserService.UserId?.ToString(),
            UserEmail = _currentUserService.Email,
            EntityName = entityType.ToString(),
            EntityId = entityId,
            Action = action.ToString(),
            OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues, options) : null,
            NewValues = newValues != null ? JsonSerializer.Serialize(newValues, options) : null,
            IpAddress = _currentUserService.IpAddress,
            UserAgent = _currentUserService.UserAgent,
            Timestamp = DateTime.UtcNow
        };

        _unitOfWork.Repository<AuditLog>().Add(auditLog);
        // Note: Do not call SaveChangesAsync here - let the caller handle the transaction
        // This prevents DbContext concurrency issues when multiple operations are in progress
        await Task.CompletedTask;
    }
}
