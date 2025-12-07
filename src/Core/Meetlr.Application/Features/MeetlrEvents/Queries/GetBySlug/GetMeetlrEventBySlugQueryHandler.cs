using System.Text.Json;
using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Domain.Entities.Users;
using Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Application.Features.MeetlrEvents.Queries.GetBySlug;

public class GetMeetlrEventBySlugQueryHandler : IRequestHandler<GetMeetlrEventBySlugQuery, GetMeetlrEventBySlugQueryResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetMeetlrEventBySlugQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<GetMeetlrEventBySlugQueryResponse> Handle(GetMeetlrEventBySlugQuery request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Repository<User>().GetQueryable()
            .FirstOrDefaultAsync(u => u.MeetlrUsername == request.Username, cancellationToken);

        if (user == null)
            throw UserErrors.UserNotFound(Guid.Empty, $"User with username '{request.Username}' not found");

        var eventType = await _unitOfWork.Repository<Domain.Entities.Events.MeetlrEvent>().GetQueryable()
            .Include(e => e.User)
            .Include(e => e.Questions)
            .Include(e => e.Theme)
            .FirstOrDefaultAsync(e => e.UserId == user.Id && e.Slug == request.Slug && e.IsActive, cancellationToken);

        if (eventType == null)
            throw MeetlrEventErrors.MeetlrEventNotFoundBySlug(request.Slug);

        return new GetMeetlrEventBySlugQueryResponse
        {
            Id = eventType.Id,
            TenantId = eventType.TenantId,
            Name = eventType.Name,
            Slug = eventType.Slug,
            Description = eventType.Description,
            DurationMinutes = eventType.DurationMinutes,
            BufferTimeBeforeMinutes = eventType.BufferTimeBeforeMinutes,
            BufferTimeAfterMinutes = eventType.BufferTimeAfterMinutes,
            MinBookingNoticeMinutes = eventType.MinBookingNoticeMinutes,
            Color = eventType.Color,
            Fee = eventType.Fee,
            Currency = eventType.Currency,
            AllowsRecurring = eventType.AllowsRecurring,
            MaxRecurringOccurrences = eventType.MaxRecurringOccurrences,
            MeetingType = eventType.MeetingType,
            Questions = eventType.Questions.Select(q => new QuestionDto
            {
                Id = q.Id,
                Question = q.QuestionText,
                Type = q.Type,
                IsRequired = q.IsRequired,
                Options = ParseOptions(q.Options)
            }).ToList(),
            Host = new HostDto
            {
                FirstName = eventType.User.FirstName,
                LastName = eventType.User.LastName,
                CompanyName = eventType.User.CompanyName,
                LogoUrl = eventType.User.LogoUrl,
                ProfileImageUrl = eventType.User.ProfileImageUrl,
                Bio = eventType.User.Bio
            },
            Theme = eventType.Theme != null ? new EventThemeDto
            {
                PrimaryColor = eventType.Theme.PrimaryColor,
                SecondaryColor = eventType.Theme.SecondaryColor,
                CalendarBackgroundColor = eventType.Theme.CalendarBackgroundColor,
                TextColor = eventType.Theme.TextColor,
                FontFamily = eventType.Theme.FontFamily,
                ButtonStyle = eventType.Theme.ButtonStyle,
                BorderRadius = eventType.Theme.BorderRadius,
                BannerImageUrl = eventType.Theme.BannerImageUrl,
                BannerText = eventType.Theme.BannerText,
                ShowHostPhoto = eventType.Theme.ShowHostPhoto,
                ShowEventDescription = eventType.Theme.ShowEventDescription,
                CustomWelcomeMessage = eventType.Theme.CustomWelcomeMessage
            } : null
        };
    }

    private static List<string> ParseOptions(string? options)
    {
        if (string.IsNullOrEmpty(options))
            return new List<string>();

        try
        {
            // Try to parse as JSON array first
            var parsed = JsonSerializer.Deserialize<List<string>>(options);
            return parsed ?? new List<string>();
        }
        catch
        {
            // Fall back to comma-separated for backwards compatibility
            return options.Split(',').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList();
        }
    }
}
