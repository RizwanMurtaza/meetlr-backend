using MediatR;

namespace Meetlr.Application.Features.SmtpConfiguration.Commands.UpdateSmtpConfiguration;

public record UpdateSmtpConfigurationCommand : IRequest<UpdateSmtpConfigurationCommandResponse>
{
    public Guid Id { get; init; }
    public string SmtpHost { get; init; } = string.Empty;
    public int SmtpPort { get; init; }
    public string SmtpUsername { get; init; } = string.Empty;
    public string? SmtpPassword { get; init; } // Optional - only update if provided
    public string FromEmail { get; init; } = string.Empty;
    public string FromName { get; init; } = string.Empty;
    public bool EnableSsl { get; init; }
}
