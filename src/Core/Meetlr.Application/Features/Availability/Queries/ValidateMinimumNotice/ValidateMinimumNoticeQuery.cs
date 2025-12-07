using MediatR;

namespace Meetlr.Application.Features.Availability.Queries.ValidateMinimumNotice;

/// <summary>
/// Query to validate that a booking meets the minimum notice requirement
/// </summary>
public class ValidateMinimumNoticeQuery : IRequest<ValidateMinimumNoticeQueryResponse>
{
    public DateTime StartTimeUtc { get; set; }
    public int MinBookingNoticeMinutes { get; set; }
}
