using Meetlr.Application.Features.EmailTemplates.Commands.DeleteEmailTemplate;

namespace Meetlr.Api.Endpoints.EmailTemplates.Delete;

public record DeleteEmailTemplateResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;

    public static DeleteEmailTemplateResponse FromCommandResponse(DeleteEmailTemplateCommandResponse response)
    {
        return new DeleteEmailTemplateResponse
        {
            Success = response.Success,
            Message = response.Message
        };
    }
}
