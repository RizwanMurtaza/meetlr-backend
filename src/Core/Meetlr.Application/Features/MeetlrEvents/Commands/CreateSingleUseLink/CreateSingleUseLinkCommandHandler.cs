using System.Security.Cryptography;
using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Common.Settings;
using Meetlr.Application.Interfaces;
using Meetlr.Domain.Entities.Events;
using Meetlr.Domain.Entities.Users;
using Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Meetlr.Application.Features.MeetlrEvents.Commands.CreateSingleUseLink;

public class CreateSingleUseLinkCommandHandler : IRequestHandler<CreateSingleUseLinkCommand, CreateSingleUseLinkCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ApplicationUrlsSettings _urlsSettings;

    public CreateSingleUseLinkCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IOptions<ApplicationUrlsSettings> urlsSettings)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _urlsSettings = urlsSettings.Value;
    }

    public async Task<CreateSingleUseLinkCommandResponse> Handle(CreateSingleUseLinkCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId!.Value;

        // Verify the event exists and belongs to the current user
        var meetlrEvent = await _unitOfWork.Repository<MeetlrEvent>()
            .GetQueryable()
            .FirstOrDefaultAsync(e => e.Id == request.MeetlrEventId && e.UserId == userId && !e.IsDeleted, cancellationToken);

        if (meetlrEvent == null)
        {
            throw MeetlrEventErrors.MeetlrEventNotFound(request.MeetlrEventId);
        }

        // Get user for building the URL
        var user = await _unitOfWork.Repository<User>()
            .GetQueryable()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            throw UserErrors.UserNotFound(userId);
        }

        // Generate unique token
        var token = GenerateSecureToken();

        var singleUseLink = new SingleUseBookingLink
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            MeetlrEventId = request.MeetlrEventId,
            Token = token,
            Name = request.Name,
            ExpiresAt = request.ExpiresAt,
            IsUsed = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _unitOfWork.Repository<SingleUseBookingLink>().Add(singleUseLink);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Build the single-use booking URL
        var bookingUrl = $"{_urlsSettings.FrontendUrl}/book/{user.MeetlrUsername}/{meetlrEvent.Slug}?token={token}";

        return new CreateSingleUseLinkCommandResponse
        {
            Id = singleUseLink.Id,
            Token = token,
            BookingUrl = bookingUrl,
            Name = singleUseLink.Name,
            ExpiresAt = singleUseLink.ExpiresAt,
            CreatedAt = singleUseLink.CreatedAt
        };
    }

    private static string GenerateSecureToken()
    {
        var bytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");
    }
}
