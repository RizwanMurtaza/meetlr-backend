using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Domain.Entities.Tenancy;
using Meetlr.Domain.Entities.Users;
using Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Application.Features.Tenants.Queries.GetTenantBySubdomain;

/// <summary>
/// Handler for getting tenant information by subdomain
/// </summary>
public class GetTenantBySubdomainHandler : IRequestHandler<GetTenantBySubdomainQuery, GetTenantBySubdomainResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetTenantBySubdomainHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<GetTenantBySubdomainResponse> Handle(
        GetTenantBySubdomainQuery request,
        CancellationToken cancellationToken)
    {
        var subdomain = request.Subdomain.ToLower();

        var tenant = await _unitOfWork.Repository<Tenant>().GetQueryable()
            .FirstOrDefaultAsync(t => t.Subdomain == subdomain && t.IsActive, cancellationToken);

        if (tenant == null)
        {
            throw TenantErrors.TenantNotFound(request.Subdomain);
        }

        // Get users for this tenant
        var usersQuery = _unitOfWork.Repository<User>().GetQueryable()
            .Where(u => u.TenantId == tenant.Id && !u.IsDeleted);

        // Filter by username if provided (for tenant.mywebsite.com/username scenario)
        if (!string.IsNullOrEmpty(request.Username))
        {
            usersQuery = usersQuery.Where(u => u.MeetlrUsername == request.Username.ToLower());
        }

        var users = await usersQuery
            .Select(u => new TenantUserDto
            {
                UserId = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                MeetlrUsername = u.MeetlrUsername,
                ProfileImageUrl = u.ProfileImageUrl,
                Bio = u.Bio
            })
            .ToListAsync(cancellationToken);

        return new GetTenantBySubdomainResponse
        {
            TenantId = tenant.Id,
            Name = tenant.Name,
            Subdomain = tenant.Subdomain,
            CustomDomain = tenant.CustomDomain,
            MainText = tenant.MainText,
            Description = tenant.Description,
            Email = tenant.Email,
            PhoneNumber = tenant.PhoneNumber,
            Website = tenant.Website,
            LogoUrl = tenant.LogoUrl,
            FaviconUrl = tenant.FaviconUrl,
            PrimaryColor = tenant.PrimaryColor,
            SecondaryColor = tenant.SecondaryColor,
            AccentColor = tenant.AccentColor,
            BackgroundColor = tenant.BackgroundColor,
            TextColor = tenant.TextColor,
            FontFamily = tenant.FontFamily,
            MetaTitle = tenant.MetaTitle,
            MetaDescription = tenant.MetaDescription,
            MetaKeywords = tenant.MetaKeywords,
            Users = users
        };
    }
}
