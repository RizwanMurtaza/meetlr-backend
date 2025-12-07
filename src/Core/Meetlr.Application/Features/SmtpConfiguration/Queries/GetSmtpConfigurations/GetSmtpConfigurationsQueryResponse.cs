namespace Meetlr.Application.Features.SmtpConfiguration.Queries.GetSmtpConfigurations;

public record GetSmtpConfigurationsQueryResponse
{
    public List<SmtpConfigurationDto> Configurations { get; init; } = new();
}