using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Interfaces;
using Meetlr.Domain.Entities.Contacts;
using Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Application.Features.Contacts.Commands.UpdateContact;

public class UpdateContactCommandHandler : IRequestHandler<UpdateContactCommand, UpdateContactCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public UpdateContactCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<UpdateContactCommandResponse> Handle(UpdateContactCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw AuthenticationErrors.UserNotAuthenticated();

        var contact = await _unitOfWork.Repository<Contact>().GetQueryable()
            .FirstOrDefaultAsync(c => c.Id == request.Id && c.UserId == userId, cancellationToken);

        if (contact == null)
        {
            throw ContactErrors.ContactNotFound(request.Id);
        }

        // Update only provided fields (partial update)
        if (request.Name != null)
            contact.Name = request.Name;

        if (request.Email != null)
        {
            // Check if new email already exists for another contact
            var emailExists = await _unitOfWork.Repository<Contact>().GetQueryable()
                .AnyAsync(c =>
                    c.Email.ToLower() == request.Email.ToLower() &&
                    c.UserId == userId &&
                    c.Id != request.Id,
                    cancellationToken);

            if (emailExists)
            {
                throw ContactErrors.ContactAlreadyExists(request.Email);
            }

            contact.Email = request.Email;
        }

        if (request.Phone != null)
            contact.Phone = request.Phone;

        if (request.TimeZone != null)
            contact.TimeZone = request.TimeZone;

        if (request.Company != null)
            contact.Company = request.Company;

        if (request.JobTitle != null)
            contact.JobTitle = request.JobTitle;

        if (request.ProfileImageUrl != null)
            contact.ProfileImageUrl = request.ProfileImageUrl;

        if (request.PreferredLanguage != null)
            contact.PreferredLanguage = request.PreferredLanguage;

        if (request.Tags != null)
            contact.Tags = request.Tags;

        if (request.MarketingConsent.HasValue)
            contact.MarketingConsent = request.MarketingConsent.Value;

        if (request.IsShared.HasValue)
            contact.IsShared = request.IsShared.Value;

        if (request.IsBlacklisted.HasValue)
            contact.IsBlacklisted = request.IsBlacklisted.Value;

        if (request.BlockedReason != null)
            contact.BlockedReason = request.BlockedReason;

        if (request.CustomFieldsJson != null)
            contact.CustomFieldsJson = request.CustomFieldsJson;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new UpdateContactCommandResponse
        {
            Id = contact.Id,
            Name = contact.Name,
            Email = contact.Email,
            UpdatedAt = contact.UpdatedAt ?? DateTime.UtcNow
        };
    }
}
