using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Interfaces;
using Meetlr.Domain.Entities.Contacts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Application.Features.Contacts.Queries.GetOrCreateContact;

public class GetOrCreateContactQueryHandler : IRequestHandler<GetOrCreateContactQuery, Contact>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantService _tenantService;

    public GetOrCreateContactQueryHandler(
        IUnitOfWork unitOfWork,
        ITenantService tenantService)
    {
        _unitOfWork = unitOfWork;
        _tenantService = tenantService;
    }

    public async Task<Contact> Handle(GetOrCreateContactQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantService.TenantId ?? throw new InvalidOperationException("Tenant not resolved");

        // Try to find existing contact by email within the tenant and user
        // Tenant filtering is automatic via global query filter
        var contact = await _unitOfWork.Repository<Contact>().GetQueryable()
            .FirstOrDefaultAsync(c =>
                c.Email.ToLower() == request.Email.ToLower() &&
                c.UserId == request.UserId,
                cancellationToken);

        if (contact != null)
        {
            // Update last contacted timestamp and increment booking count
            contact.LastContactedAt = DateTime.UtcNow;
            contact.TotalBookings++;

            // Update contact info if provided (in case details changed)
            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                contact.Name = request.Name;
            }
            if (!string.IsNullOrWhiteSpace(request.Phone))
            {
                contact.Phone = request.Phone;
            }
            if (!string.IsNullOrWhiteSpace(request.TimeZone))
            {
                contact.TimeZone = request.TimeZone;
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return contact;
        }

        // Create new contact
        contact = new Contact
        {
            Email = request.Email,
            Name = request.Name,
            Phone = request.Phone,
            TimeZone = request.TimeZone,
            UserId = request.UserId,
            Source = request.Source,
            LastContactedAt = DateTime.UtcNow,
            TotalBookings = 1,
            IsShared = false,
            IsBlacklisted = false,
            MarketingConsent = false,
            NoShowCount = 0
            // TenantId will be auto-injected by SaveChangesAsync
        };

        _unitOfWork.Repository<Contact>().Add(contact);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return contact;
    }
}
