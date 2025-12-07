using MediatR;
using Microsoft.Extensions.Logging;

namespace Meetlr.Application.Features.Availability.Queries.ValidateMinimumNotice;

/// <summary>
/// Query handler for validating minimum booking notice requirement
/// </summary>
public class ValidateMinimumNoticeQueryHandler
    : IRequestHandler<ValidateMinimumNoticeQuery, ValidateMinimumNoticeQueryResponse>
{
    private readonly ILogger<ValidateMinimumNoticeQueryHandler> _logger;

    public ValidateMinimumNoticeQueryHandler(ILogger<ValidateMinimumNoticeQueryHandler> logger)
    {
        _logger = logger;
    }

    public Task<ValidateMinimumNoticeQueryResponse> Handle(
        ValidateMinimumNoticeQuery request,
        CancellationToken cancellationToken)
    {
        var minimumTime = DateTime.UtcNow.AddMinutes(request.MinBookingNoticeMinutes);

        if (request.StartTimeUtc < minimumTime)
        {
            _logger.LogWarning(
                "Booking does not meet minimum notice: Requested {StartTime}, Minimum required {MinimumTime} ({NoticeMinutes} minutes)",
                request.StartTimeUtc,
                minimumTime,
                request.MinBookingNoticeMinutes);

            return Task.FromResult(new ValidateMinimumNoticeQueryResponse
            {
                IsValid = false,
                ErrorMessage = $"Booking requires at least {request.MinBookingNoticeMinutes} minutes advance notice"
            });
        }

        _logger.LogInformation("Minimum notice requirement met");

        return Task.FromResult(new ValidateMinimumNoticeQueryResponse
        {
            IsValid = true,
            ErrorMessage = null
        });
    }
}
