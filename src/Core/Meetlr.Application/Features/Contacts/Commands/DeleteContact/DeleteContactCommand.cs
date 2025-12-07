using MediatR;

namespace Meetlr.Application.Features.Contacts.Commands.DeleteContact;

public record DeleteContactCommand : IRequest<DeleteContactCommandResponse>
{
    public Guid Id { get; init; }
}
