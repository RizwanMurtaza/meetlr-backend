using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Domain.Entities.Events;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Application.Features.MeetlrEvents.Queries.GetTheme;

public class GetEventThemeQueryHandler : IRequestHandler<GetEventThemeQuery, GetEventThemeResponse?>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetEventThemeQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<GetEventThemeResponse?> Handle(GetEventThemeQuery request, CancellationToken cancellationToken)
    {
        // Get theme for the event (no user check - themes are public for booking pages)
        var theme = await _unitOfWork.Repository<EventTheme>()
            .GetQueryable()
            .FirstOrDefaultAsync(t => t.MeetlrEventId == request.MeetlrEventId && !t.IsDeleted, cancellationToken);

        if (theme == null)
        {
            return null; // Return null if no custom theme exists (frontend will use defaults)
        }

        return new GetEventThemeResponse
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
