using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Interfaces;
using Meetlr.Domain.Entities.Scheduling;
using Meetlr.Domain.Enums;
using Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Application.Features.Schedule.Commands.UpdateSingleDayAvailability;

public class UpdateSingleDayCommandHandler : IRequestHandler<UpdateSingleDayCommandRequest, UpdateSingleDayCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;

    public UpdateSingleDayCommandHandler(
        IUnitOfWork unitOfWork,
        IAuditService auditService)
    {
        _unitOfWork = unitOfWork;
        _auditService = auditService;
    }

    public async Task<UpdateSingleDayCommandResponse> Handle(UpdateSingleDayCommandRequest request, CancellationToken cancellationToken)
    {
        // Verify schedule exists and user has permission
        var schedule = await _unitOfWork.Repository<AvailabilitySchedule>().GetQueryable()
            .Include(a => a.WeeklyHours)
            .FirstOrDefaultAsync(a => a.Id == request.ScheduleId && a.UserId == request.UserId, cancellationToken);

        if (schedule == null)
        {
            throw AvailabilityErrors.ScheduleNotFoundOrNoPermission();
        }

        // Find existing weekly hours for this day
        var existingHours = schedule.WeeklyHours
            .Where(w => w.DayOfWeek == request.DayOfWeek)
            .ToList();

        var oldData = existingHours.Select(h => new
        {
            h.DayOfWeek,
            h.StartTime,
            h.EndTime,
            h.IsAvailable
        }).ToList();

        // Remove all existing hours for this day
        foreach (var hour in existingHours)
        {
            _unitOfWork.Repository<WeeklyHours>().Delete(hour);
        }

        // Add new weekly hours for this day (supports multiple time slots)
        if (request.IsAvailable && request.TimeSlots.Count > 0)
        {
            // Validate time slots don't overlap
            var sortedSlots = request.TimeSlots.OrderBy(s => s.StartTime).ToList();
            for (int i = 0; i < sortedSlots.Count - 1; i++)
            {
                if (sortedSlots[i].EndTime > sortedSlots[i + 1].StartTime)
                {
                    return new UpdateSingleDayCommandResponse
                    {
                        ScheduleId = schedule.Id,
                        DayOfWeek = (int)request.DayOfWeek,
                        Success = false,
                        Message = "Time slots cannot overlap"
                    };
                }
            }

            // Add each time slot
            foreach (var slot in request.TimeSlots)
            {
                var weeklyHour = new WeeklyHours
                {
                    Id = Guid.NewGuid(),
                    AvailabilityScheduleId = schedule.Id,
                    DayOfWeek = request.DayOfWeek,
                    StartTime = slot.StartTime,
                    EndTime = slot.EndTime,
                    IsAvailable = true,
                    CreatedAt = DateTime.UtcNow
                };

                _unitOfWork.Repository<WeeklyHours>().Add(weeklyHour);
            }
        }

        // Update schedule's UpdatedAt timestamp
        schedule.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Log audit
        var newData = request.TimeSlots.Select(s => new
        {
            request.DayOfWeek,
            s.StartTime,
            s.EndTime,
            IsAvailable = request.IsAvailable
        }).ToList();

        await _auditService.LogAsync(
            AuditEntityType.AvailabilitySchedule,
            schedule.Id.ToString(),
            AuditAction.Update,
            oldData,
            newData,
            cancellationToken);

        return new UpdateSingleDayCommandResponse
        {
            ScheduleId = schedule.Id,
            DayOfWeek = (int)request.DayOfWeek,
            Success = true,
            Message = request.IsAvailable && request.TimeSlots.Count > 0
                ? $"Day availability updated with {request.TimeSlots.Count} time slot(s)"
                : "Day marked as unavailable"
        };
    }
}
