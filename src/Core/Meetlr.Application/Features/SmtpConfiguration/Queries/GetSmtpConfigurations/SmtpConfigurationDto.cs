namespace Meetlr.Application.Features.SmtpConfiguration.Queries.GetSmtpConfigurations;

public record SmtpConfigurationDto
{
    public Guid Id { get; init; }
    public string SmtpHost { get; init; } = string.Empty;
    public int SmtpPort { get; init; }
    public string SmtpUsername { get; init; } = string.Empty;
    public string FromEmail { get; init; } = string.Empty;
    public string FromName { get; init; } = string.Empty;
    public bool EnableSsl { get; init; }
    public bool IsActive { get; init; }
    public string Level { get; init; } = string.Empty; // "System", "Tenant", "User"
    public DateTime? LastTestedAt { get; init; }
    public bool? LastTestSucceeded { get; init; }
    public string? LastTestError { get; init; }
}