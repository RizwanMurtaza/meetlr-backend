using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Interfaces;
using Meetlr.Domain.Entities.Contacts;
using Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Application.Features.Contacts.Commands.CreateContact;

public class CreateContactCommandHandler : IRequestHandler<CreateContactCommand, CreateContactCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CreateContactCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<CreateContactCommandResponse> Handle(CreateContactCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw AuthenticationErrors.UserNotAuthenticated();

        // Check if contact with same email already exists for this user
        var existingContact = await _unitOfWork.Repository<Contact>().GetQueryable()
            .FirstOrDefaultAsync(c =>
                c.Email.ToLower() == request.Email.ToLower() &&
                c.UserId == userId,
                cancellationToken);

        if (existingContact != null)
        {
            throw ContactErrors.ContactAlreadyExists(request.Email);
        }

        var contact = new Contact
        {
            UserId = userId,
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            TimeZone = request.TimeZone,
            Company = request.Company,
            JobTitle = request.JobTitle,
            ProfileImageUrl = request.ProfileImageUrl,
            PreferredLanguage = request.PreferredLanguage,
            Tags = request.Tags,
            MarketingConsent = request.MarketingConsent,
            Source = request.Source,
            IsShared = request.IsShared,
            CustomFieldsJson = request.CustomFieldsJson,
            TotalBookings = 0,
            NoShowCount = 0,
            IsBlacklisted = false
        };

        _unitOfWork.Repository<Contact>().Add(contact);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateContactCommandResponse
        {
            Id = contact.Id,
            Name = contact.Name,
            Email = contact.Email,
            CreatedAt = contact.CreatedAt
        };
    }
}
