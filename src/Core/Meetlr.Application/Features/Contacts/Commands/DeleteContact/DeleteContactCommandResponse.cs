namespace Meetlr.Application.Features.Contacts.Commands.DeleteContact;

public record DeleteContactCommandResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
}
