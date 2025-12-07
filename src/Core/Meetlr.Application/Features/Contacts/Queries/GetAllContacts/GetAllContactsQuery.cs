using MediatR;

namespace Meetlr.Application.Features.Contacts.Queries.GetAllContacts;

public record GetAllContactsQuery : IRequest<GetAllContactsQueryResponse>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? SearchTerm { get; init; } // Search by name or email
    public bool? IsBlacklisted { get; init; } // Filter by blacklist status
    public string? Source { get; init; } // Filter by source
    public string? SortBy { get; init; } = "LastContactedAt"; // Name, Email, LastContactedAt, TotalBookings, CreatedAt
    public bool SortDescending { get; init; } = true;
}
