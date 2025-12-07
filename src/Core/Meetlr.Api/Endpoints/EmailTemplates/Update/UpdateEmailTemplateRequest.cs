using Meetlr.Application.Features.EmailTemplates.Commands.UpdateEmailTemplate;

namespace Meetlr.Api.Endpoints.EmailTemplates.Update;

public record UpdateEmailTemplateRequest
{
    public string Subject { get; init; } = string.Empty;
    public string HtmlBody { get; init; } = string.Empty;
    public string? PlainTextBody { get; init; }
    public bool IsActive { get; init; } = true;

    public static UpdateEmailTemplateCommand ToCommand(UpdateEmailTemplateRequest request, Guid id)
    {
        return new UpdateEmailTemplateCommand
        {
            Id = id,
            Subject = request.Subject,
            HtmlBody = request.HtmlBody,
            PlainTextBody = request.PlainTextBody,
            IsActive = request.IsActive
        };
    }
}
