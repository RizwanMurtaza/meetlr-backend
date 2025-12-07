using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Interfaces;
using Meetlr.Domain.Enums;
using Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Application.Features.MeetlrEvents.Commands.Update;

public class UpdateMeetlrEventCommandHandler : IRequestHandler<UpdateMeetlrEventCommand, UpdateMeetlrEventCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;

    public UpdateMeetlrEventCommandHandler(
        IUnitOfWork unitOfWork,
        IAuditService auditService)
    {
        _unitOfWork = unitOfWork;
        _auditService = auditService;
    }

    public async Task<UpdateMeetlrEventCommandResponse> Handle(UpdateMeetlrEventCommand request, CancellationToken cancellationToken)
    {
        var eventType = await _unitOfWork.Repository<Domain.Entities.Events.MeetlrEvent>().GetQueryable()
            .FirstOrDefaultAsync(e => e.Id == request.Id && e.UserId == request.UserId, cancellationToken);

        if (eventType == null)
            throw MeetlrEventErrors.NotMeetlrEventOwner("Event type not found or you don't have permission to update it");

        var oldValues = new
        {
            eventType.Name,
            eventType.Color,
            eventType.NotifyViaEmail,
            eventType.NotifyViaSms,
            eventType.NotifyViaWhatsApp
        };

        // Only update the 5 allowed fields (true PATCH behavior)
        if (request.Name != null)
            eventType.Name = request.Name;
        if (request.Color != null)
            eventType.Color = request.Color;
        if (request.NotifyViaEmail.HasValue)
            eventType.NotifyViaEmail = request.NotifyViaEmail.Value;
        if (request.NotifyViaSms.HasValue)
            eventType.NotifyViaSms = request.NotifyViaSms.Value;
        if (request.NotifyViaWhatsApp.HasValue)
            eventType.NotifyViaWhatsApp = request.NotifyViaWhatsApp.Value;

        eventType.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Log audit
        await _auditService.LogAsync(
            AuditEntityType.MeetlrEvent,
            eventType.Id.ToString(),
            AuditAction.Update,
            oldValues,
            eventType,
            cancellationToken);

        return new UpdateMeetlrEventCommandResponse
        {
            Id = eventType.Id,
            Name = eventType.Name,
            Slug = eventType.Slug,
            Success = true
        };
    }
}
