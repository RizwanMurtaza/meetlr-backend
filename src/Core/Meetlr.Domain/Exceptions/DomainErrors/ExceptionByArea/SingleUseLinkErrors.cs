using Meetlr.Domain.Exceptions.Base;
using Meetlr.Domain.Exceptions.Http;

namespace Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;

/// <summary>
/// Defines errors related to single-use booking link management.
/// All errors use ApplicationArea.SingleUseLinks (area code: 14)
/// </summary>
public static class SingleUseLinkErrors
{
    private const ApplicationArea Area = ApplicationArea.SingleUseLinks;

    public static NotFoundException LinkNotFound(Guid linkId, string? customMessage = null)
    {
        var exception = new NotFoundException(Area, 1, customMessage ?? "Single-use booking link not found");
        exception.WithDetail("LinkId", linkId);
        return exception;
    }

    public static NotFoundException LinkNotFoundByToken(string token, string? customMessage = null)
    {
        var exception = new NotFoundException(Area, 2, customMessage ?? "Single-use booking link not found");
        exception.WithDetail("Token", token);
        return exception;
    }

    public static BadRequestException LinkAlreadyUsed(string token, string? customMessage = null)
    {
        var exception = new BadRequestException(Area, 3, customMessage ?? "This booking link has already been used");
        exception.WithDetail("Token", token);
        return exception;
    }

    public static BadRequestException LinkExpired(string token, string? customMessage = null)
    {
        var exception = new BadRequestException(Area, 4, customMessage ?? "This booking link has expired");
        exception.WithDetail("Token", token);
        return exception;
    }

    public static BadRequestException LinkInactive(string token, string? customMessage = null)
    {
        var exception = new BadRequestException(Area, 5, customMessage ?? "This booking link is no longer active");
        exception.WithDetail("Token", token);
        return exception;
    }

    public static ForbiddenException NotLinkOwner(string? customMessage = null)
        => new(Area, 6, customMessage ?? "You do not have permission to manage this booking link");
}
