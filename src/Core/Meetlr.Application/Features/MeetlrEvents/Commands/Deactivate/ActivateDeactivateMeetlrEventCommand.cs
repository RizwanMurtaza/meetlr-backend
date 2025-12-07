using MediatR;

namespace Meetlr.Application.Features.MeetlrEvents.Commands.Deactivate;

/// <summary>
/// Command to deactivate an event type with optional future bookings cancellation
/// </summary>
public record ActivateDeactivateMeetlrEventCommand : IRequest<ActivateDeactivateMeetlrEventCommandResponse>
{
    public Guid MeetlrEventId { get; init; }
    public Guid UserId { get; init; }
    
    public bool IsActive { get; init; }
    public bool CancelFutureBookings { get; init; }
    public string? CancellationReason { get; init; }
}
