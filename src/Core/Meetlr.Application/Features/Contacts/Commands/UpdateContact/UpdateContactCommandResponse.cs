namespace Meetlr.Application.Features.Contacts.Commands.UpdateContact;

public record UpdateContactCommandResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public DateTime UpdatedAt { get; init; }
}
