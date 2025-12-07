using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Interfaces;
using Meetlr.Domain.Entities.Events;
using Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Application.Features.MeetlrEvents.Commands.CreateOrUpdateTheme;

public class CreateOrUpdateEventThemeCommandHandler : IRequestHandler<CreateOrUpdateEventThemeCommand, CreateOrUpdateEventThemeResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CreateOrUpdateEventThemeCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<CreateOrUpdateEventThemeResponse> Handle(CreateOrUpdateEventThemeCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId!.Value;

        // Verify the event exists and belongs to the current user
        var meetlrEvent = await _unitOfWork.Repository<Domain.Entities.Events.MeetlrEvent>()
            .GetQueryable()
            .FirstOrDefaultAsync(e => e.Id == request.MeetlrEventId && e.UserId == userId && !e.IsDeleted, cancellationToken);

        if (meetlrEvent == null)
        {
            throw MeetlrEventErrors.MeetlrEventNotFound(request.MeetlrEventId);
        }

        // Check if theme already exists for this event
        var existingTheme = await _unitOfWork.Repository<EventTheme>()
            .GetQueryable()
            .FirstOrDefaultAsync(t => t.MeetlrEventId == request.MeetlrEventId && !t.IsDeleted, cancellationToken);

        EventTheme theme;
        bool isNew = existingTheme == null;

        if (isNew)
        {
            // Create new theme
            theme = new EventTheme
            {
                Id = Guid.NewGuid(),
                MeetlrEventId = request.MeetlrEventId,
                CreatedAt = DateTime.UtcNow
            };
        }
        else
        {
            theme = existingTheme!;
            theme.UpdatedAt = DateTime.UtcNow;
        }

        // Update all properties
        theme.PrimaryColor = request.PrimaryColor;
        theme.SecondaryColor = request.SecondaryColor;
        theme.CalendarBackgroundColor = request.CalendarBackgroundColor;
        theme.TextColor = request.TextColor;
        theme.FontFamily = request.FontFamily;
        theme.ButtonStyle = request.ButtonStyle;
        theme.BorderRadius = request.BorderRadius;
        theme.BannerImageUrl = request.BannerImageUrl;
        theme.BannerText = request.BannerText;
        theme.ShowHostPhoto = request.ShowHostPhoto;
        theme.ShowEventDescription = request.ShowEventDescription;
        theme.CustomWelcomeMessage = request.CustomWelcomeMessage;

        if (isNew)
        {
            _unitOfWork.Repository<EventTheme>().Add(theme);
        }
        else
        {
            _unitOfWork.Repository<EventTheme>().Update(theme);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateOrUpdateEventThemeResponse
        {
            Id = theme.Id,
            MeetlrEventId = theme.MeetlrEventId,
            PrimaryColor = theme.PrimaryColor,
            SecondaryColor = theme.SecondaryColor,
            CalendarBackgroundColor = theme.CalendarBackgroundColor,
            TextColor = theme.TextColor,
            FontFamily = theme.FontFamily,
            ButtonStyle = theme.ButtonStyle,
            BorderRadius = theme.BorderRadius,
            BannerImageUrl = theme.BannerImageUrl,
            BannerText = theme.BannerText,
            ShowHostPhoto = theme.ShowHostPhoto,
            ShowEventDescription = theme.ShowEventDescription,
            CustomWelcomeMessage = theme.CustomWelcomeMessage,
            CreatedAt = theme.CreatedAt,
            UpdatedAt = theme.UpdatedAt
        };
    }
}
