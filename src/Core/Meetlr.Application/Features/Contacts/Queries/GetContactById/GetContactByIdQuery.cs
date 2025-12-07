using MediatR;

namespace Meetlr.Application.Features.Contacts.Queries.GetContactById;

public record GetContactByIdQuery : IRequest<GetContactByIdQueryResponse>
{
    public Guid Id { get; init; }
}
