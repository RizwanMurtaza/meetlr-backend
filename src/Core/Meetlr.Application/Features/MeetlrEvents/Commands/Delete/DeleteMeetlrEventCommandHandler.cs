using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Interfaces;
using Meetlr.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Application.Features.MeetlrEvents.Commands.Delete;

public class DeleteMeetlrEventCommandHandler : IRequestHandler<DeleteMeetlrEventCommand, DeleteMeetlrEventCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;

    public DeleteMeetlrEventCommandHandler(
        IUnitOfWork unitOfWork,
        IAuditService auditService)
    {
        _unitOfWork = unitOfWork;
        _auditService = auditService;
    }

    public async Task<DeleteMeetlrEventCommandResponse> Handle(DeleteMeetlrEventCommand request, CancellationToken cancellationToken)
    {
        var eventType = await _unitOfWork.Repository<Domain.Entities.Events.MeetlrEvent>().GetQueryable()
            .FirstOrDefaultAsync(e => e.Id == request.Id && e.UserId == request.UserId, cancellationToken);

        if (eventType == null)
        {
            return new DeleteMeetlrEventCommandResponse
            {
                Success = false,
                Message = "Event type not found or you don't have permission to delete it"
            };
        }

        // Soft delete - just mark as inactive
        eventType.IsActive = false;
        eventType.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Log audit
        await _auditService.LogAsync(
            AuditEntityType.MeetlrEvent,
            eventType.Id.ToString(),
            AuditAction.Delete,
            eventType,
            null,
            cancellationToken);

        return new DeleteMeetlrEventCommandResponse
        {
            Success = true,
            Message = "Event type deleted successfully"
        };
    }
}
