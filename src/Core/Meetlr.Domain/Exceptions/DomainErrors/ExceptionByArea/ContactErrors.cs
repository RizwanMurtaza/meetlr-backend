using Meetlr.Domain.Exceptions.Base;
using Meetlr.Domain.Exceptions.Http;

namespace Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;

/// <summary>
/// Defines errors related to contact management.
/// All errors use ApplicationArea.Contacts (area code: 15)
/// </summary>
public static class ContactErrors
{
    private const ApplicationArea Area = ApplicationArea.Contacts;

    public static NotFoundException ContactNotFound(Guid contactId, string? customMessage = null)
    {
        var exception = new NotFoundException(Area, 1, customMessage ?? "Contact not found");
        exception.WithDetail("ContactId", contactId);
        return exception;
    }

    public static ConflictException ContactAlreadyExists(string email, string? customMessage = null)
    {
        var exception = new ConflictException(Area, 2, customMessage ?? $"Contact with email '{email}' already exists");
        exception.WithDetail("Email", email);
        return exception;
    }

    public static BadRequestException InvalidContactEmail(string email, string? customMessage = null)
    {
        var exception = new BadRequestException(Area, 3, customMessage ?? "Invalid contact email format");
        exception.WithDetail("Email", email);
        return exception;
    }

    public static ForbiddenException NotContactOwner(string? customMessage = null)
        => new(Area, 4, customMessage ?? "You do not have permission to access this contact");
}
