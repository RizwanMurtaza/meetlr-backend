using MediatR;

namespace Meetlr.Application.Features.SmtpConfiguration.Commands.TestSmtpConfiguration;

public record TestSmtpConfigurationCommand : IRequest<TestSmtpConfigurationCommandResponse>
{
    public Guid Id { get; init; }
}
