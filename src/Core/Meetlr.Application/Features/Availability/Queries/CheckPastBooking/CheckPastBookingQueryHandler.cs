using MediatR;
using Microsoft.Extensions.Logging;

namespace Meetlr.Application.Features.Availability.Queries.CheckPastBooking;

/// <summary>
/// Query handler for checking if booking time is in the past
/// </summary>
public class CheckPastBookingQueryHandler
    : IRequestHandler<CheckPastBookingQuery, CheckPastBookingQueryResponse>
{
    private readonly ILogger<CheckPastBookingQueryHandler> _logger;

    public CheckPastBookingQueryHandler(ILogger<CheckPastBookingQueryHandler> logger)
    {
        _logger = logger;
    }

    public Task<CheckPastBookingQueryResponse> Handle(
        CheckPastBookingQuery request,
        CancellationToken cancellationToken)
    {
        if (request.StartTimeUtc < DateTime.UtcNow)
        {
            _logger.LogWarning(
                "Booking time is in the past: {StartTime} (Current: {CurrentTime})",
                request.StartTimeUtc,
                DateTime.UtcNow);

            return Task.FromResult(new CheckPastBookingQueryResponse
            {
                IsInPast = true,
                ErrorMessage = "Cannot book a time slot in the past"
            });
        }

        _logger.LogInformation("Booking time is valid (future)");

        return Task.FromResult(new CheckPastBookingQueryResponse
        {
            IsInPast = false,
            ErrorMessage = null
        });
    }
}
