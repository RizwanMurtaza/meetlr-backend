using Meetlr.Application.Features.SmtpConfiguration.Commands.UpdateSmtpConfiguration;

namespace Meetlr.Api.Endpoints.SmtpConfiguration.Update;

public record UpdateSmtpConfigurationResponse
{
    public Guid Id { get; init; }
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;

    public static UpdateSmtpConfigurationResponse FromCommandResponse(UpdateSmtpConfigurationCommandResponse response)
    {
        return new UpdateSmtpConfigurationResponse
        {
            Id = response.Id,
            Success = response.Success,
            Message = response.Message
        };
    }
}
