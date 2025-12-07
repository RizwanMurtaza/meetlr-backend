using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Domain.Entities.Users;
using Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Application.Features.MeetlrEvents.Queries.GetTypesByUsername;

public class GetEventTypesByUsernameQueryHandler : IRequestHandler<GetMeetlrEventsByUsernameQuery, GetMeetlrEventsByUsernameQueryResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetEventTypesByUsernameQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<GetMeetlrEventsByUsernameQueryResponse> Handle(GetMeetlrEventsByUsernameQuery request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Repository<User>().GetQueryable()
            .FirstOrDefaultAsync(u => u.MeetlrUsername == request.Username, cancellationToken);

        if (user == null)
            throw UserErrors.UserNotFound(Guid.Empty, $"User with username '{request.Username}' not found");

        var eventTypes = await _unitOfWork.Repository<Domain.Entities.Events.MeetlrEvent>().GetQueryable()
            .Where(e => e.UserId == user.Id && e.IsActive)
            .OrderBy(e => e.CreatedAt)
            .ToListAsync(cancellationToken);

        return new GetMeetlrEventsByUsernameQueryResponse
        {
            MeetlrEvents = eventTypes.Select(e => new MeetlrEventListItem
            {
                Id = e.Id,
                Name = e.Name,
                Slug = e.Slug,
                Description = e.Description,
                DurationMinutes = e.DurationMinutes,
                Color = e.Color,
                Location = e.LocationDetails,
                Fee = e.Fee,
                Currency = e.Currency,
                AllowsRecurring = e.AllowsRecurring,
                MaxRecurringOccurrences = e.MaxRecurringOccurrences,
                IsActive = e.IsActive
            }).ToList(),
            Host = new HostInfo
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                CompanyName = user.CompanyName,
                LogoUrl = user.LogoUrl
            }
        };
    }
}