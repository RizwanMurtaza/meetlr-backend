using Meetlr.Domain.Exceptions.Base;
using Meetlr.Domain.Exceptions.Http;

namespace Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;

/// <summary>
/// Defines errors related to slot invitation management.
/// All errors use ApplicationArea.SlotInvitations (area code: 19)
/// </summary>
public static class SlotInvitationErrors
{
    private const ApplicationArea Area = ApplicationArea.SlotInvitations;

    public static NotFoundException InvitationNotFound(Guid invitationId, string? customMessage = null)
    {
        var exception = new NotFoundException(Area, 1, customMessage ?? "Slot invitation not found");
        exception.WithDetail("InvitationId", invitationId);
        return exception;
    }

    public static NotFoundException InvitationNotFoundByToken(string token, string? customMessage = null)
    {
        var exception = new NotFoundException(Area, 2, customMessage ?? "Slot invitation not found or has expired");
        exception.WithDetail("Token", token);
        return exception;
    }

    public static BadRequestException InvitationExpired(Guid invitationId, string? customMessage = null)
    {
        var exception = new BadRequestException(Area, 3, customMessage ?? "This slot invitation has expired");
        exception.WithDetail("InvitationId", invitationId);
        return exception;
    }

    public static BadRequestException InvitationAlreadyBooked(Guid invitationId, string? customMessage = null)
    {
        var exception = new BadRequestException(Area, 4, customMessage ?? "This slot invitation has already been booked");
        exception.WithDetail("InvitationId", invitationId);
        return exception;
    }

    public static BadRequestException CannotDeleteBookedInvitation(Guid invitationId, string? customMessage = null)
    {
        var exception = new BadRequestException(Area, 5, customMessage ?? "Cannot delete an invitation that has been booked");
        exception.WithDetail("InvitationId", invitationId);
        return exception;
    }

    public static BadRequestException CannotResendEmail(Guid invitationId, string? customMessage = null)
    {
        var exception = new BadRequestException(Area, 6, customMessage ?? "Cannot resend email for this invitation. Maximum attempts reached or invitation is no longer pending.");
        exception.WithDetail("InvitationId", invitationId);
        return exception;
    }

    public static ForbiddenException NotInvitationOwner(string? customMessage = null)
        => new(Area, 7, customMessage ?? "You do not have permission to access this slot invitation");
}
