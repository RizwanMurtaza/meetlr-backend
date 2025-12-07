using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Interfaces;
using Meetlr.Domain.Entities.Tenancy;
using Meetlr.Domain.Entities.Users;
using Meetlr.Domain.Enums;
using Meetlr.Domain.Exceptions.DomainErrors.ExceptionByArea;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Application.Features.Tenants.Commands.UpdateBranding;

/// <summary>
/// Handler for updating tenant branding
/// </summary>
public class UpdateBrandingHandler : IRequestHandler<UpdateBrandingCommand, UpdateBrandingResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;

    public UpdateBrandingHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IAuditService auditService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _auditService = auditService;
    }

    public async Task<UpdateBrandingResponse> Handle(
        UpdateBrandingCommand request,
        CancellationToken cancellationToken)
    {
        // Get current user's tenant
        var currentUserId = _currentUserService.UserId ?? throw AuthenticationErrors.UserNotAuthenticated();

        var currentUser = await _unitOfWork.Repository<User>().GetQueryable()
            .Include(u => u.UserGroups)
            .ThenInclude(ug => ug.Group)
            .FirstOrDefaultAsync(u => u.Id == currentUserId, cancellationToken);

        if (currentUser == null)
        {
            throw UserErrors.UserNotFound(currentUserId);
        }

        // Verify user belongs to the tenant
        if (currentUser.TenantId != request.TenantId)
        {
            throw UserErrors.NotTenantMember("You do not have permission to update this tenant's branding");
        }

        // Verify user is in admin group
        var isAdmin = currentUser.UserGroups.Any(ug => ug.Group.IsAdminGroup && ug.IsAdmin);
        if (!isAdmin)
        {
            throw AuthenticationErrors.NotAdminUser("Only admin users can update tenant branding");
        }

        // Get tenant
        var tenant = await _unitOfWork.Repository<Tenant>().GetQueryable()
            .FirstOrDefaultAsync(t => t.Id == request.TenantId, cancellationToken);

        if (tenant == null)
        {
            throw TenantErrors.TenantNotFoundById(request.TenantId);
        }

        // Store old values for audit
        var oldValues = new
        {
            tenant.LogoUrl,
            tenant.FaviconUrl,
            tenant.PrimaryColor,
            tenant.SecondaryColor,
            tenant.AccentColor,
            tenant.MainText,
            tenant.Description
        };

        // Update branding properties
        if (request.LogoUrl != null) tenant.LogoUrl = request.LogoUrl;
        if (request.FaviconUrl != null) tenant.FaviconUrl = request.FaviconUrl;
        if (request.PrimaryColor != null) tenant.PrimaryColor = request.PrimaryColor;
        if (request.SecondaryColor != null) tenant.SecondaryColor = request.SecondaryColor;
        if (request.AccentColor != null) tenant.AccentColor = request.AccentColor;
        if (request.BackgroundColor != null) tenant.BackgroundColor = request.BackgroundColor;
        if (request.TextColor != null) tenant.TextColor = request.TextColor;
        if (request.FontFamily != null) tenant.FontFamily = request.FontFamily;
        if (request.MainText != null) tenant.MainText = request.MainText;
        if (request.Description != null) tenant.Description = request.Description;
        if (request.MetaTitle != null) tenant.MetaTitle = request.MetaTitle;
        if (request.MetaDescription != null) tenant.MetaDescription = request.MetaDescription;
        if (request.MetaKeywords != null) tenant.MetaKeywords = request.MetaKeywords;

        tenant.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Log audit
        await _auditService.LogAsync(
            AuditEntityType.Tenant,
            tenant.Id.ToString(),
            AuditAction.UpdateBranding,
            oldValues,
            tenant,
            cancellationToken);

        return new UpdateBrandingResponse
        {
            TenantId = tenant.Id,
            TenantName = tenant.Name,
            Message = "Tenant branding updated successfully",
            UpdatedAt = tenant.UpdatedAt.Value
        };
    }
}
