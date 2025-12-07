using Meetlr.Application.Features.EmailTemplates.Commands.CopyEmailTemplate;

namespace Meetlr.Api.Endpoints.EmailTemplates.Copy;

public record CopyEmailTemplateResponse
{
    public Guid Id { get; init; }
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;

    public static CopyEmailTemplateResponse FromCommandResponse(CopyEmailTemplateCommandResponse response)
    {
        return new CopyEmailTemplateResponse
        {
            Id = response.Id,
            Success = response.Success,
            Message = response.Message
        };
    }
}
