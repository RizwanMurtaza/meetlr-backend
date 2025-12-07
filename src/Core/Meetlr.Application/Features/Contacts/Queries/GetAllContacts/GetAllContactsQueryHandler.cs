using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Interfaces;
using Meetlr.Domain.Entities.Contacts;
using Meetlr.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Application.Features.Contacts.Queries.GetAllContacts;

public class GetAllContactsQueryHandler : IRequestHandler<GetAllContactsQuery, GetAllContactsQueryResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetAllContactsQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<GetAllContactsQueryResponse> Handle(GetAllContactsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException("User not authenticated");

        var query = _unitOfWork.Repository<Contact>().GetQueryable()
            .Where(c => c.UserId == userId);

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchLower = request.SearchTerm.ToLower();
            query = query.Where(c =>
                c.Name.ToLower().Contains(searchLower) ||
                c.Email.ToLower().Contains(searchLower) ||
                (c.Company != null && c.Company.ToLower().Contains(searchLower)));
        }

        // Apply blacklist filter
        if (request.IsBlacklisted.HasValue)
        {
            query = query.Where(c => c.IsBlacklisted == request.IsBlacklisted.Value);
        }

        // Apply source filter
        if (!string.IsNullOrWhiteSpace(request.Source) && Enum.TryParse<ContactSource>(request.Source, out var sourceEnum))
        {
            query = query.Where(c => c.Source == sourceEnum);
        }

        // Apply sorting
        query = request.SortBy?.ToLower() switch
        {
            "name" => request.SortDescending
                ? query.OrderByDescending(c => c.Name)
                : query.OrderBy(c => c.Name),
            "email" => request.SortDescending
                ? query.OrderByDescending(c => c.Email)
                : query.OrderBy(c => c.Email),
            "totalbookings" => request.SortDescending
                ? query.OrderByDescending(c => c.TotalBookings)
                : query.OrderBy(c => c.TotalBookings),
            "createdat" => request.SortDescending
                ? query.OrderByDescending(c => c.CreatedAt)
                : query.OrderBy(c => c.CreatedAt),
            "lastcontactedat" or _ => request.SortDescending
                ? query.OrderByDescending(c => c.LastContactedAt)
                : query.OrderBy(c => c.LastContactedAt)
        };

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
        var contacts = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new ContactDto
            {
                Id = c.Id,
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
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        return new GetAllContactsQueryResponse
        {
            Contacts = contacts,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalPages = totalPages,
            HasPreviousPage = request.PageNumber > 1,
            HasNextPage = request.PageNumber < totalPages
        };
    }
}
