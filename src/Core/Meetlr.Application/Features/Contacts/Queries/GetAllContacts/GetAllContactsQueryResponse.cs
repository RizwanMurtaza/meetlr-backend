namespace Meetlr.Application.Features.Contacts.Queries.GetAllContacts;

public record GetAllContactsQueryResponse
{
    public List<ContactDto> Contacts { get; init; } = new();
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
    public bool HasPreviousPage { get; init; }
    public bool HasNextPage { get; init; }
}
