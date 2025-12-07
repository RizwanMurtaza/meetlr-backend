using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Interfaces;
using Meetlr.Domain.Entities.Scheduling;
using Meetlr.Domain.Enums;
using Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Application.Features.Schedule.Commands.UpdateAvailabilitySchedule;

public class UpdateAvailabilityScheduleCommandHandler : IRequestHandler<UpdateAvailabilityScheduleCommand, UpdateAvailabilityScheduleCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;

    public UpdateAvailabilityScheduleCommandHandler(
        IUnitOfWork unitOfWork,
        IAuditService auditService)
    {
        _unitOfWork = unitOfWork;
        _auditService = auditService;
    }

    public async Task<UpdateAvailabilityScheduleCommandResponse> Handle(UpdateAvailabilityScheduleCommand request, CancellationToken cancellationToken)
    {
        var schedule = await _unitOfWork.Repository<AvailabilitySchedule>().GetQueryable()
            .Include(a => a.WeeklyHours)
            .FirstOrDefaultAsync(a => a.Id == request.Id && a.UserId == request.UserId, cancellationToken);

        if (schedule == null)
        {
            throw AvailabilityErrors.ScheduleNotFoundOrNoPermission();
        }

        var oldSchedule = new
        {
            schedule.Name,
            schedule.TimeZone,
            schedule.IsDefault
        };

        // Check if timezone is changing and if new timezone already exists
        if (schedule.TimeZone != request.TimeZone)
        {
            var existingScheduleWithTimezone = await _unitOfWork.Repository<AvailabilitySchedule>().GetQueryable()
                .AnyAsync(a => a.UserId == request.UserId && a.TimeZone == request.TimeZone && a.Id != request.Id, cancellationToken);

            if (existingScheduleWithTimezone)
            {
                throw AvailabilityErrors.TimezoneScheduleAlreadyExists(request.TimeZone);
            }
        }

        // If this is set as default, unset other defaults for this user
        if (request.IsDefault && !schedule.IsDefault)
        {
            var existingDefaults = await _unitOfWork.Repository<AvailabilitySchedule>().GetQueryable()
                .Where(a => a.UserId == request.UserId && a.IsDefault && a.Id != request.Id)
                .ToListAsync(cancellationToken);

            foreach (var defaultSchedule in existingDefaults)
            {
                defaultSchedule.IsDefault = false;
            }
        }

        // Update schedule
        schedule.Name = request.Name;
        schedule.TimeZone = request.TimeZone;
        schedule.IsDefault = request.IsDefault;
        schedule.ScheduleType = request.ScheduleType;
        schedule.MaxBookingsPerSlot = request.MaxBookingsPerSlot;
        // Advanced Settings
        schedule.MaxBookingDaysInFuture = request.MaxBookingDaysInFuture;
        schedule.MinBookingNoticeMinutes = request.MinBookingNoticeMinutes;
        schedule.SlotIntervalMinutes = request.SlotIntervalMinutes;
        schedule.AutoDetectInviteeTimezone = request.AutoDetectInviteeTimezone;
        schedule.UpdatedAt = DateTime.UtcNow;

        // Remove existing weekly hours
        var existingWeeklyHours = schedule.WeeklyHours.ToList();
        foreach (var existingHour in existingWeeklyHours)
        {
            _unitOfWork.Repository<WeeklyHours>().Delete(existingHour);
        }

        // Add new weekly hours
        foreach (var hourDto in request.WeeklyHours)
        {
            var weeklyHour = new WeeklyHours
            {
                Id = Guid.NewGuid(),
                AvailabilityScheduleId = schedule.Id,
                DayOfWeek = hourDto.DayOfWeek,
                StartTime = hourDto.StartTime,
                EndTime = hourDto.EndTime,
                IsAvailable = hourDto.IsAvailable,
                CreatedAt = DateTime.UtcNow
            };

            _unitOfWork.Repository<WeeklyHours>().Add(weeklyHour);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Log audit
        await _auditService.LogAsync(
            AuditEntityType.AvailabilitySchedule,
            schedule.Id.ToString(),
            AuditAction.Update,
            oldSchedule,
            schedule,
            cancellationToken);

        return new UpdateAvailabilityScheduleCommandResponse
        {
            Id = schedule.Id,
            Name = schedule.Name,
            TimeZone = schedule.TimeZone,
            IsDefault = schedule.IsDefault,
            Success = true
        };
    }
}
