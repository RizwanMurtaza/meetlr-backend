using Meetlr.Application.Features.EmailTemplates.Commands.UpdateEmailTemplate;

namespace Meetlr.Api.Endpoints.EmailTemplates.Update;

public record UpdateEmailTemplateResponse
{
    public Guid Id { get; init; }
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;

    public static UpdateEmailTemplateResponse FromCommandResponse(UpdateEmailTemplateCommandResponse response)
    {
        return new UpdateEmailTemplateResponse
        {
            Id = response.Id,
            Success = response.Success,
            Message = response.Message
        };
    }
}
