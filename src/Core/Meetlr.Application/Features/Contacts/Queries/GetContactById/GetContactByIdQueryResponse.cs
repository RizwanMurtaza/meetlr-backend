namespace Meetlr.Application.Features.Contacts.Queries.GetContactById;

public record GetContactByIdQueryResponse
{
    public ContactDetailDto? Contact { get; init; }
}
