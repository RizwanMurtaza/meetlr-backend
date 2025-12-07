namespace Meetlr.Application.Features.EmailTemplates.Commands.CopyEmailTemplate;

public record CopyEmailTemplateCommandResponse
{
    public Guid Id { get; init; }
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
}
