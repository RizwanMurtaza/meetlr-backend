using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Common.Settings;
using Meetlr.Application.Interfaces;
using Meetlr.Domain.Entities.Events;
using Meetlr.Domain.Entities.Users;
using Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Meetlr.Application.Features.MeetlrEvents.Queries.GetSingleUseLinks;

public class GetSingleUseLinksQueryHandler : IRequestHandler<GetSingleUseLinksQuery, GetSingleUseLinksQueryResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ApplicationUrlsSettings _urlsSettings;

    public GetSingleUseLinksQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IOptions<ApplicationUrlsSettings> urlsSettings)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _urlsSettings = urlsSettings.Value;
    }

    public async Task<GetSingleUseLinksQueryResponse> Handle(GetSingleUseLinksQuery request, CancellationToken cancellationToken)
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

        // Get user for building URLs
        var user = await _unitOfWork.Repository<User>()
            .GetQueryable()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        var query = _unitOfWork.Repository<SingleUseBookingLink>()
            .GetQueryable()
            .Where(l => l.MeetlrEventId == request.MeetlrEventId && l.IsActive && !l.IsDeleted);

        // Filter by used status if specified
        if (request.IncludeUsed != true)
        {
            query = query.Where(l => !l.IsUsed);
        }

        var links = await query
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync(cancellationToken);

        var now = DateTime.UtcNow;
        var baseUrl = $"{_urlsSettings.FrontendUrl}/book/{user?.MeetlrUsername}/{meetlrEvent.Slug}";

        return new GetSingleUseLinksQueryResponse
        {
            Links = links.Select(l => new SingleUseLinkItem
            {
                Id = l.Id,
                Token = l.Token,
                BookingUrl = $"{baseUrl}?token={l.Token}",
                Name = l.Name,
                IsUsed = l.IsUsed,
                UsedAt = l.UsedAt,
                ExpiresAt = l.ExpiresAt,
                IsExpired = l.ExpiresAt.HasValue && l.ExpiresAt.Value < now,
                IsActive = l.IsActive,
                CreatedAt = l.CreatedAt
            }).ToList()
        };
    }
}
