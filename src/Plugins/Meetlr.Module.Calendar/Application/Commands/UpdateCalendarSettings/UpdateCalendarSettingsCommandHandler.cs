using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Module.Calendar.Application.Queries.GetConnectedCalendars;
using Meetlr.Module.Calendar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Meetlr.Module.Calendar.Application.Commands.UpdateCalendarSettings;

public class UpdateCalendarSettingsCommandHandler
    : IRequestHandler<UpdateCalendarSettingsCommand, UpdateCalendarSettingsCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateCalendarSettingsCommandHandler> _logger;

    public UpdateCalendarSettingsCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<UpdateCalendarSettingsCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<UpdateCalendarSettingsCommandResponse> Handle(
        UpdateCalendarSettingsCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Get the calendar integration
            var calendarIntegration = await _unitOfWork.Repository<CalendarIntegration>()
                .GetQueryable()
                .FirstOrDefaultAsync(
                    c => c.Id == request.CalendarIntegrationId && c.AvailabilityScheduleId == request.ScheduleId,
                    cancellationToken);

            if (calendarIntegration == null)
            {
                _logger.LogWarning(
                    "Calendar integration not found. CalendarIntegrationId: {CalendarIntegrationId}, ScheduleId: {ScheduleId}",
                    request.CalendarIntegrationId,
                    request.ScheduleId);

                return new UpdateCalendarSettingsCommandResponse
                {
                    Success = false,
                    Message = "Calendar integration not found"
                };
            }

            // If setting this as primary calendar, unset other primary calendars for this schedule
            if (request.IsPrimaryCalendar)
            {
                var otherCalendars = await _unitOfWork.Repository<CalendarIntegration>()
                    .GetQueryable()
                    .Where(c => c.AvailabilityScheduleId == request.ScheduleId && c.Id != request.CalendarIntegrationId && c.IsPrimaryCalendar)
                    .ToListAsync(cancellationToken);

                foreach (var calendar in otherCalendars)
                {
                    calendar.IsPrimaryCalendar = false;
                    _unitOfWork.Repository<CalendarIntegration>().Update(calendar);
                }

                _logger.LogInformation(
                    "Unset {Count} other primary calendars for schedule {ScheduleId}",
                    otherCalendars.Count,
                    request.ScheduleId);
            }

            // Update settings
            calendarIntegration.IsPrimaryCalendar = request.IsPrimaryCalendar;
            calendarIntegration.CheckForConflicts = request.CheckForConflicts;
            calendarIntegration.AddEventsToCalendar = request.AddEventsToCalendar;
            calendarIntegration.IncludeBuffers = request.IncludeBuffers;
            calendarIntegration.AutoSync = request.AutoSync;
            calendarIntegration.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<CalendarIntegration>().Update(calendarIntegration);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Calendar settings updated successfully. CalendarIntegrationId: {CalendarIntegrationId}, " +
                "IsPrimary: {IsPrimary}, CheckConflicts: {CheckConflicts}, AddEvents: {AddEvents}, " +
                "IncludeBuffers: {IncludeBuffers}, AutoSync: {AutoSync}",
                request.CalendarIntegrationId,
                request.IsPrimaryCalendar,
                request.CheckForConflicts,
                request.AddEventsToCalendar,
                request.IncludeBuffers,
                request.AutoSync);

            // Return the updated integration data
            var integrationDto = new CalendarIntegrationDto(
                calendarIntegration.Id,
                calendarIntegration.AvailabilityScheduleId,
                calendarIntegration.Provider.ToString(),
                calendarIntegration.ProviderEmail,
                !string.IsNullOrEmpty(calendarIntegration.AccessToken),
                calendarIntegration.LastSyncedAt,
                calendarIntegration.IsPrimaryCalendar,
                calendarIntegration.CheckForConflicts,
                calendarIntegration.AddEventsToCalendar,
                calendarIntegration.IncludeBuffers,
                calendarIntegration.AutoSync
            );

            return new UpdateCalendarSettingsCommandResponse
            {
                Success = true,
                Message = "Calendar settings updated successfully",
                CalendarIntegrationId = calendarIntegration.Id,
                Integration = integrationDto
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error updating calendar settings. CalendarIntegrationId: {CalendarIntegrationId}, ScheduleId: {ScheduleId}",
                request.CalendarIntegrationId,
                request.ScheduleId);

            return new UpdateCalendarSettingsCommandResponse
            {
                Success = false,
                Message = $"Error updating calendar settings: {ex.Message}"
            };
        }
    }
}
