using MediatR;
using Meetlr.Application.Interfaces;

namespace Meetlr.Application.Features.Availability.Queries.ConvertBookingTimeToUtc;

/// <summary>
/// Pure CQRS query handler for converting booking times to UTC
/// </summary>
public class ConvertBookingTimeToUtcQueryHandler : IRequestHandler<ConvertBookingTimeToUtcQuery, ConvertBookingTimeToUtcQueryResponse>
{
    private readonly ITimeZoneService _timeZoneService;

    public ConvertBookingTimeToUtcQueryHandler(ITimeZoneService timeZoneService)
    {
        _timeZoneService = timeZoneService;
    }

    public Task<ConvertBookingTimeToUtcQueryResponse> Handle(ConvertBookingTimeToUtcQuery request, CancellationToken cancellationToken)
    {
        var startTimeUtc = request.RequestedTime.Kind == DateTimeKind.Utc
            ? request.RequestedTime
            : _timeZoneService.ConvertToUtc(request.RequestedTime, request.TimeZone);

        var endTimeUtc = startTimeUtc.AddMinutes(request.DurationMinutes);

        return Task.FromResult(new ConvertBookingTimeToUtcQueryResponse
        {
            StartTimeUtc = startTimeUtc,
            EndTimeUtc = endTimeUtc
        });
    }
}
