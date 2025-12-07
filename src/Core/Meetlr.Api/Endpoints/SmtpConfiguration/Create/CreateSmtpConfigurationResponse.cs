using Meetlr.Application.Features.SmtpConfiguration.Commands.CreateSmtpConfiguration;

namespace Meetlr.Api.Endpoints.SmtpConfiguration.Create;

public record CreateSmtpConfigurationResponse
{
    public Guid Id { get; init; }
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;

    public static CreateSmtpConfigurationResponse FromCommandResponse(CreateSmtpConfigurationCommandResponse response)
    {
        return new CreateSmtpConfigurationResponse
        {
            Id = response.Id,
            Success = response.Success,
            Message = response.Message
        };
    }
}
