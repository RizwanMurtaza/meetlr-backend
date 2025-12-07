using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Interfaces;
using Meetlr.Domain.Entities.Scheduling;
using Meetlr.Domain.Enums;
using Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Application.Features.Schedule.Commands.UpdateAdvancedSettings;

public class UpdateAdvancedSettingsCommandHandler : IRequestHandler<UpdateAdvancedSettingsCommand, UpdateAdvancedSettingsCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;

    public UpdateAdvancedSettingsCommandHandler(
        IUnitOfWork unitOfWork,
        IAuditService auditService)
    {
        _unitOfWork = unitOfWork;
        _auditService = auditService;
    }

    public async Task<UpdateAdvancedSettingsCommandResponse> Handle(UpdateAdvancedSettingsCommand request, CancellationToken cancellationToken)
    {
        var schedule = await _unitOfWork.Repository<AvailabilitySchedule>().GetQueryable()
            .FirstOrDefaultAsync(a => a.Id == request.ScheduleId && a.UserId == request.UserId, cancellationToken);

        if (schedule == null)
        {
            throw AvailabilityErrors.ScheduleNotFoundOrNoPermission();
        }

        var oldValues = new
        {
            schedule.MaxBookingDaysInFuture,
            schedule.MinBookingNoticeMinutes,
            schedule.SlotIntervalMinutes,
            schedule.AutoDetectInviteeTimezone
        };

        // Update only advanced settings
        schedule.MaxBookingDaysInFuture = request.MaxBookingDaysInFuture;
        schedule.MinBookingNoticeMinutes = request.MinBookingNoticeMinutes;
        schedule.SlotIntervalMinutes = request.SlotIntervalMinutes;
        schedule.AutoDetectInviteeTimezone = request.AutoDetectInviteeTimezone;
        schedule.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Log audit
        await _auditService.LogAsync(
            AuditEntityType.AvailabilitySchedule,
            schedule.Id.ToString(),
            AuditAction.Update,
            oldValues,
            new
            {
                schedule.MaxBookingDaysInFuture,
                schedule.MinBookingNoticeMinutes,
                schedule.SlotIntervalMinutes,
                schedule.AutoDetectInviteeTimezone
            },
            cancellationToken);

        return new UpdateAdvancedSettingsCommandResponse
        {
            Success = true,
            MaxBookingDaysInFuture = schedule.MaxBookingDaysInFuture,
            MinBookingNoticeMinutes = schedule.MinBookingNoticeMinutes,
            SlotIntervalMinutes = schedule.SlotIntervalMinutes,
            AutoDetectInviteeTimezone = schedule.AutoDetectInviteeTimezone
        };
    }
}
