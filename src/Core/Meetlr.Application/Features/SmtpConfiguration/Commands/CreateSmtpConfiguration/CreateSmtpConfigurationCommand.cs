using MediatR;

namespace Meetlr.Application.Features.SmtpConfiguration.Commands.CreateSmtpConfiguration;

public record CreateSmtpConfigurationCommand : IRequest<CreateSmtpConfigurationCommandResponse>
{
    public Guid? TenantId { get; init; }
    public Guid? UserId { get; init; }
    public string SmtpHost { get; init; } = string.Empty;
    public int SmtpPort { get; init; }
    public string SmtpUsername { get; init; } = string.Empty;
    public string SmtpPassword { get; init; } = string.Empty;
    public string FromEmail { get; init; } = string.Empty;
    public string FromName { get; init; } = string.Empty;
    public bool EnableSsl { get; init; } = true;
}
