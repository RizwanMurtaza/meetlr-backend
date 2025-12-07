using Meetlr.Domain.Entities.Events;

namespace Meetlr.Application.Common.Interfaces;

public interface IRecurrenceOccurrenceGenerator
{
    List<DateTime> GenerateOccurrences(BookingSeries series, DateTime startDate, int maxOccurrences);
}
