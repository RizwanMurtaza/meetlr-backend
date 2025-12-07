using Meetlr.Domain.Common;

namespace Meetlr.Application.Common.Interfaces;

/// <summary>
/// Service for dispatching domain events via MediatR
/// </summary>
public interface IDomainEventDispatcher
{
    /// <summary>
    /// Dispatch all domain events from entities with pending events
    /// </summary>
    Task DispatchEventsAsync(IEnumerable<IHasDomainEvents> entitiesWithEvents, CancellationToken cancellationToken = default);
}
