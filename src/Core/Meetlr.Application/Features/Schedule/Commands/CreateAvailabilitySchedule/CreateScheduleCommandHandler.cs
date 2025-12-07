using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Interfaces;
using Meetlr.Domain.Entities.Scheduling;
using Meetlr.Domain.Enums;
using Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;
using Microsoft.EntityFrameworkCore;
using UserSettingsEntity = Meetlr.Domain.Entities.Users.UserSettings;

namespace Meetlr.Application.Features.Schedule.Commands.CreateAvailabilitySchedule;

public class CreateScheduleCommandHandler : IRequestHandler<CreateScheduleCommandRequest, CreateScheduleCommandResponse>
{
    private const int MaxSchedulesPerUser = 3;

    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;

    public CreateScheduleCommandHandler(
        IUnitOfWork unitOfWork,
        IAuditService auditService)
    {
        _unitOfWork = unitOfWork;
        _auditService = auditService;
    }

    public async Task<CreateScheduleCommandResponse> Handle(CreateScheduleCommandRequest request, CancellationToken cancellationToken)
    {
        // Check if user has reached the maximum number of schedules (limit: 3)
        var existingScheduleCount = await _unitOfWork.Repository<AvailabilitySchedule>().GetQueryable()
            .CountAsync(a => a.UserId == request.UserId, cancellationToken);

        if (existingScheduleCount >= MaxSchedulesPerUser)
        {
            throw AvailabilityErrors.MaxScheduleLimitReached(MaxSchedulesPerUser);
        }

        // If this is set as default, unset other defaults for this user
        if (request.IsDefault)
        {
            var existingDefaults = await _unitOfWork.Repository<AvailabilitySchedule>().GetQueryable()
                .Where(a => a.UserId == request.UserId && a.IsDefault)
                .ToListAsync(cancellationToken);

            foreach (var schedule in existingDefaults)
            {
                schedule.IsDefault = false;
            }
        }

        // Fetch user settings to populate advanced settings defaults
        var userSettings = await _unitOfWork.Repository<UserSettingsEntity>().GetQueryable()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.UserId == request.UserId, cancellationToken);

        // Create availability schedule with advanced settings from user defaults
        var availabilitySchedule = new AvailabilitySchedule
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Name = request.Name,
            TimeZone = request.TimeZone,
            IsDefault = request.IsDefault,
            ScheduleType = request.ScheduleType,
            MaxBookingsPerSlot = request.MaxBookingsPerSlot,
            // Advanced Settings - populated from UserSettings defaults
            MaxBookingDaysInFuture = userSettings?.DefaultMaxBookingDaysInFuture ?? 60,
            MinBookingNoticeMinutes = userSettings?.DefaultMinBookingNotice ?? 60,
            SlotIntervalMinutes = userSettings?.DefaultSlotIntervalMinutes ?? 15,
            AutoDetectInviteeTimezone = userSettings?.AutoDetectInviteeTimezone ?? true,
            CreatedAt = DateTime.UtcNow
        };

        _unitOfWork.Repository<AvailabilitySchedule>().Add(availabilitySchedule);

        // Add weekly hours
        foreach (var hourDto in request.WeeklyHours)
        {
            var weeklyHour = new WeeklyHours
            {
                Id = Guid.NewGuid(),
                AvailabilityScheduleId = availabilitySchedule.Id,
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
            availabilitySchedule.Id.ToString(),
            AuditAction.Create,
            null,
            availabilitySchedule,
            cancellationToken);

        return new CreateScheduleCommandResponse
        {
            Id = availabilitySchedule.Id,
            Name = availabilitySchedule.Name,
            TimeZone = availabilitySchedule.TimeZone,
            IsDefault = availabilitySchedule.IsDefault,
            Success = true
        };
    }
}
