using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Features.Availability.Queries.ValidateBookingSlots;
using Meetlr.Domain.Entities.Events;
using Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Api.Endpoints.Public.CheckSlotAvailability;

[ApiController]
[Route("api/public/availability/meetrlevent")]
[AllowAnonymous]
public class CheckSlotAvailability : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IUnitOfWork _unitOfWork;

    public CheckSlotAvailability(IMediator mediator, IUnitOfWork unitOfWork)
    {
        _mediator = mediator;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Check if specific time slots are available (supports batch checking)
    /// Uses unified validation that checks both database bookings and external calendar conflicts
    /// </summary>
    [HttpPost("{meetlrEventId}/check-slots")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(List<SlotAvailabilityResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Handle(
        [FromRoute] Guid meetlrEventId,
        [FromBody] CheckSlotAvailabilityBatchRequest request,
        CancellationToken cancellationToken = default)
    {
        request.MeetlrEventId = meetlrEventId;
        if (request.Slots == null || !request.Slots.Any())
        {
            return BadRequest("At least one slot must be provided");
        }

        // Get event type to get duration if not provided
        var eventType = await _unitOfWork.Repository<MeetlrEvent>()
            .GetQueryable()
            .FirstOrDefaultAsync(e => e.Id == meetlrEventId, cancellationToken);

        if (eventType == null)
            throw MeetlrEventErrors.MeetlrEventNotFound(request.MeetlrEventId);

        // Extract all requested slots as UTC times
        var requestedSlots = request.Slots
            .Select(slot => slot.StartTime)
            .ToList();

        // Use unified validation query (checks database bookings + calendar conflicts + capacity)
        var validateQuery = new ValidateBookingSlotsQuery
        {
            MeetlrEventId = meetlrEventId,
            RequestedSlots = requestedSlots,
            TimeZone = request.Slots.FirstOrDefault()?.TimeZone ?? "UTC"
        };

        var validation = await _mediator.Send(validateQuery, cancellationToken);

        // Map validation results back to SlotAvailabilityResponse format
        var results = new List<SlotAvailabilityResponse>();

        for (int i = 0; i < request.Slots.Count; i++)
        {
            var slot = request.Slots[i];
            var endTime = slot.StartTime.AddMinutes(slot.DurationMinutes > 0 ? slot.DurationMinutes : eventType.DurationMinutes);

            // Check if this slot has conflicts
            var hasConflict = validation.Conflicts.Any(c => c.SlotIndex == i);

            results.Add(new SlotAvailabilityResponse
            {
                StartTime = slot.StartTime,
                EndTime = endTime,
                IsAvailable = !hasConflict,
                TimeZone = slot.TimeZone ?? "UTC"
            });
        }

        return Ok(results);
    }
}
