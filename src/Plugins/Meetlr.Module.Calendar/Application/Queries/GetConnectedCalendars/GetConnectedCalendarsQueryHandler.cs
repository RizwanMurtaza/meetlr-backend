using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Interfaces;
using Meetlr.Domain.Entities.Scheduling;
using Meetlr.Domain.Exceptions.Http;
using Meetlr.Module.Calendar.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Module.Calendar.Application.Queries.GetConnectedCalendars;

public class GetConnectedCalendarsQueryHandler : IRequestHandler<GetConnectedCalendarsQuery, GetConnectedCalendarsResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetConnectedCalendarsQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<GetConnectedCalendarsResponse> Handle(
        GetConnectedCalendarsQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        if (!userId.HasValue || userId.Value == Guid.Empty)
        {
            throw UnauthorizedException.NotAuthenticated();
        }

        // Verify the user owns this schedule
        var schedule = await _unitOfWork.Repository<AvailabilitySchedule>()
            .GetQueryable()
            .FirstOrDefaultAsync(s => s.Id == request.ScheduleId && s.UserId == userId.Value, cancellationToken);

        if (schedule == null)
        {
            throw ForbiddenException.NotResourceOwner("schedule");
        }

        var integrations = await _unitOfWork.Repository<CalendarIntegration>().GetQueryable()
            .Where(ci => ci.AvailabilityScheduleId == request.ScheduleId && ci.IsActive)
            .Select(ci => new CalendarIntegrationDto(
                ci.Id,
                ci.AvailabilityScheduleId,
                ci.Provider.ToString(),
                ci.ProviderEmail ?? "",
                !string.IsNullOrEmpty(ci.AccessToken),
                ci.LastSyncedAt,
                ci.IsPrimaryCalendar,
                ci.CheckForConflicts,
                ci.AddEventsToCalendar,
                ci.IncludeBuffers,
                ci.AutoSync
            ))
            .ToListAsync(cancellationToken);

        return new GetConnectedCalendarsResponse(integrations);
    }
}
