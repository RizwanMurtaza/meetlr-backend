namespace Meetlr.Domain.Common;

/// <summary>
/// Interface for entities that can raise domain events.
/// </summary>
public interface IHasDomainEvents
{
    /// <summary>
    /// Collection of domain events raised by this entity
    /// </summary>
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

    /// <summary>
    /// Add a domain event to be dispatched
    /// </summary>
    void AddDomainEvent(IDomainEvent domainEvent);

    /// <summary>
    /// Clear all domain events (typically called after dispatching)
    /// </summary>
    void ClearDomainEvents();
}
