namespace Meetlr.Application.Features.EmailTemplates.Commands.DeleteEmailTemplate;

public record DeleteEmailTemplateCommandResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
}
