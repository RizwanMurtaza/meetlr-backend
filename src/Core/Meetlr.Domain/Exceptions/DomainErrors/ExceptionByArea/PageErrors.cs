using Meetlr.Domain.Exceptions.Base;
using Meetlr.Domain.Exceptions.Http;

namespace Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;

/// <summary>
/// Defines errors related to page/homepage management.
/// All errors use ApplicationArea.Pages (area code: 13)
/// </summary>
public static class PageErrors
{
    private const ApplicationArea Area = ApplicationArea.Pages;

    public static NotFoundException PageNotFound(Guid pageId, string? customMessage = null)
    {
        var exception = new NotFoundException(Area, 1, customMessage ?? "Page not found");
        exception.WithDetail("PageId", pageId);
        return exception;
    }

    public static NotFoundException TemplateNotFound(Guid templateId, string? customMessage = null)
    {
        var exception = new NotFoundException(Area, 2, customMessage ?? "Template not found or is not active");
        exception.WithDetail("TemplateId", templateId);
        return exception;
    }

    public static ConflictException UsernameAlreadyTaken(string username, string? customMessage = null)
    {
        var exception = new ConflictException(Area, 3, customMessage ?? $"Username '{username}' is already taken");
        exception.WithDetail("Username", username);
        return exception;
    }

    public static ForbiddenException NotPageOwner(string? customMessage = null)
        => new(Area, 4, customMessage ?? "You do not have permission to modify this page");

    public static BadRequestException InvalidPageContent(string? customMessage = null)
        => new(Area, 5, customMessage ?? "Page content is invalid");
}
