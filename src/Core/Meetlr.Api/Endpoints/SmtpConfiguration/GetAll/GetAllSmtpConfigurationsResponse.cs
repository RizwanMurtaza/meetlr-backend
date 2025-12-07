using Meetlr.Application.Features.SmtpConfiguration.Queries.GetSmtpConfigurations;

namespace Meetlr.Api.Endpoints.SmtpConfiguration.GetAll;

public record GetAllSmtpConfigurationsResponse
{
    public List<SmtpConfigurationDto> Configurations { get; init; } = new();

    public static GetAllSmtpConfigurationsResponse FromQueryResponse(GetSmtpConfigurationsQueryResponse response)
    {
        return new GetAllSmtpConfigurationsResponse
        {
            Configurations = response.Configurations
        };
    }
}
