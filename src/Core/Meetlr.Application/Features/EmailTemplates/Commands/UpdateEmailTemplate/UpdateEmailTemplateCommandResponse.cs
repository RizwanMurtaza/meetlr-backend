namespace Meetlr.Application.Features.EmailTemplates.Commands.UpdateEmailTemplate;

public record UpdateEmailTemplateCommandResponse
{
    public Guid Id { get; init; }
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
}
