using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Domain.Common;
using Microsoft.Extensions.Logging;

namespace Meetlr.Infrastructure.Services;

/// <summary>
/// Dispatches domain events via MediatR after entities are saved
/// </summary>
public class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IMediator _mediator;
    private readonly ILogger<DomainEventDispatcher> _logger;

    public DomainEventDispatcher(IMediator mediator, ILogger<DomainEventDispatcher> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task DispatchEventsAsync(IEnumerable<IHasDomainEvents> entitiesWithEvents, CancellationToken cancellationToken = default)
    {
        var domainEvents = entitiesWithEvents
            .SelectMany(e => e.DomainEvents)
            .ToList();

        if (!domainEvents.Any())
            return;

        // Clear events from entities first (before dispatching to avoid re-processing)
        foreach (var entity in entitiesWithEvents)
        {
            entity.ClearDomainEvents();
        }

        // Dispatch each event
        foreach (var domainEvent in domainEvents)
        {
            _logger.LogDebug("Dispatching domain event {EventType}", domainEvent.GetType().Name);

            try
            {
                await _mediator.Publish(domainEvent, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error dispatching domain event {EventType}", domainEvent.GetType().Name);
                // Don't rethrow - domain events should not fail the main transaction
                // The event handlers should be resilient and handle their own errors
            }
        }

        _logger.LogInformation("Dispatched {Count} domain events", domainEvents.Count);
    }
}
