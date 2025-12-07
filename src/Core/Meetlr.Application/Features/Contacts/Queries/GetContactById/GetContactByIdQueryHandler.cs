using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Interfaces;
using Meetlr.Domain.Entities.Contacts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Application.Features.Contacts.Queries.GetContactById;

public class GetContactByIdQueryHandler : IRequestHandler<GetContactByIdQuery, GetContactByIdQueryResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetContactByIdQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<GetContactByIdQueryResponse> Handle(GetContactByIdQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException("User not authenticated");

        var contact = await _unitOfWork.Repository<Contact>().GetQueryable()
            .Where(c => c.Id == request.Id && c.UserId == userId)
            .Select(c => new ContactDetailDto
            {
                Id = c.Id,
                UserId = c.UserId,
                Name = c.Name,
                Email = c.Email,
                Phone = c.Phone,
                TimeZone = c.TimeZone,
                Company = c.Company,
                JobTitle = c.JobTitle,
                ProfileImageUrl = c.ProfileImageUrl,
                PreferredLanguage = c.PreferredLanguage,
                Tags = c.Tags,
                Source = c.Source,
                IsShared = c.IsShared,
                IsBlacklisted = c.IsBlacklisted,
                BlockedReason = c.BlockedReason,
                MarketingConsent = c.MarketingConsent,
                TotalBookings = c.TotalBookings,
                NoShowCount = c.NoShowCount,
                LastContactedAt = c.LastContactedAt,
                CustomFieldsJson = c.CustomFieldsJson,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (contact == null)
        {
            throw new KeyNotFoundException($"Contact with ID {request.Id} not found");
        }

        return new GetContactByIdQueryResponse
        {
            Contact = contact
        };
    }
}
