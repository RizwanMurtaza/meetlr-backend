using MediatR;

namespace Meetlr.Application.Features.SmtpConfiguration.Commands.DeleteSmtpConfiguration;

public record DeleteSmtpConfigurationCommand : IRequest<DeleteSmtpConfigurationCommandResponse>
{
    public Guid Id { get; init; }
}
