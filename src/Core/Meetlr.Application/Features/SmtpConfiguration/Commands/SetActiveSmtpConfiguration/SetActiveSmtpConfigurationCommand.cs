using MediatR;

namespace Meetlr.Application.Features.SmtpConfiguration.Commands.SetActiveSmtpConfiguration;

public record SetActiveSmtpConfigurationCommand : IRequest<SetActiveSmtpConfigurationCommandResponse>
{
    public Guid Id { get; init; }
}
