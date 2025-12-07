using Meetlr.Application.Features.SmtpConfiguration.Commands.CreateSmtpConfiguration;

namespace Meetlr.Api.Endpoints.SmtpConfiguration.Create;

public record CreateSmtpConfigurationRequest
{
    public string SmtpHost { get; init; } = string.Empty;
    public int SmtpPort { get; init; }
    public string SmtpUsername { get; init; } = string.Empty;
    public string SmtpPassword { get; init; } = string.Empty;
    public string FromEmail { get; init; } = string.Empty;
    public string FromName { get; init; } = string.Empty;
    public bool EnableSsl { get; init; } = true;
    public bool IsAdmin { get; init; }

    public static CreateSmtpConfigurationCommand ToCommand(CreateSmtpConfigurationRequest request, Guid? currentTenantId, Guid currentUserId)
    {
        return new CreateSmtpConfigurationCommand
        {
            TenantId = request.IsAdmin ? currentTenantId : null,
            UserId = request.IsAdmin ? null : currentUserId,
            SmtpHost = request.SmtpHost,
            SmtpPort = request.SmtpPort,
            SmtpUsername = request.SmtpUsername,
            SmtpPassword = request.SmtpPassword,
            FromEmail = request.FromEmail,
            FromName = request.FromName,
            EnableSsl = request.EnableSsl
        };
    }
}
