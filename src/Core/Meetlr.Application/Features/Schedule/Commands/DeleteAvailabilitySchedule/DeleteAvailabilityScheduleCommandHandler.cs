using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Interfaces;
using Meetlr.Domain.Entities.Scheduling;
using Meetlr.Domain.Enums;
using Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Application.Features.Schedule.Commands.DeleteAvailabilitySchedule;

public class DeleteAvailabilityScheduleCommandHandler : IRequestHandler<DeleteAvailabilityScheduleCommand, DeleteAvailabilityScheduleCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;

    public DeleteAvailabilityScheduleCommandHandler(
        IUnitOfWork unitOfWork,
        IAuditService auditService)
    {
        _unitOfWork = unitOfWork;
        _auditService = auditService;
    }

    public async Task<DeleteAvailabilityScheduleCommandResponse> Handle(DeleteAvailabilityScheduleCommand request, CancellationToken cancellationToken)
    {
        var schedule = await _unitOfWork.Repository<AvailabilitySchedule>().GetQueryable()
            .Include(a => a.WeeklyHours)
            .Include(a => a.DateSpecificHours)
            .Include(a => a.MeetlrEvents)
            .FirstOrDefaultAsync(a => a.Id == request.Id && a.UserId == request.UserId, cancellationToken);

        if (schedule == null)
        {
            throw AvailabilityErrors.ScheduleNotFoundOrNoPermission();
        }

        // Check if any event types are using this schedule
        if (schedule.MeetlrEvents.Any())
        {
            throw AvailabilityErrors.ScheduleInUse();
        }

        // Delete related weekly hours
        foreach (var weeklyHour in schedule.WeeklyHours.ToList())
        {
            _unitOfWork.Repository<WeeklyHours>().Delete(weeklyHour);
        }

        // Delete related date specific hours
        foreach (var dateSpecificHour in schedule.DateSpecificHours.ToList())
        {
            _unitOfWork.Repository<DateSpecificHours>().Delete(dateSpecificHour);
        }

        // Delete the schedule
        _unitOfWork.Repository<AvailabilitySchedule>().Delete(schedule);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Log audit
        await _auditService.LogAsync(
            AuditEntityType.AvailabilitySchedule,
            schedule.Id.ToString(),
            AuditAction.Delete,
            schedule,
            null,
            cancellationToken);

        return new DeleteAvailabilityScheduleCommandResponse
        {
            Success = true
        };
    }
}
