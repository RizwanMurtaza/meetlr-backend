using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Interfaces;
using Meetlr.Domain.Entities.Emails;
using Meetlr.Domain.Exceptions.Http;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Application.Features.EmailTemplates.Commands.CopyEmailTemplate;

public class CopyEmailTemplateCommandHandler : IRequestHandler<CopyEmailTemplateCommand, CopyEmailTemplateCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailTemplateRenderer _templateRenderer;
    private readonly ICurrentUserService _currentUserService;

    public CopyEmailTemplateCommandHandler(
        IUnitOfWork unitOfWork,
        IEmailTemplateRenderer templateRenderer,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _templateRenderer = templateRenderer;
        _currentUserService = currentUserService;
    }

    public async Task<CopyEmailTemplateCommandResponse> Handle(CopyEmailTemplateCommand request, CancellationToken cancellationToken)
    {
        // Check if user already has this template type
        var existingTemplate = await _unitOfWork.Repository<EmailTemplate>()
            .GetQueryable()
            .FirstOrDefaultAsync(t =>
                t.TemplateType == request.TemplateType &&
                t.TenantId == request.TenantId &&
                t.UserId == request.UserId &&
                !t.IsDeleted,
                cancellationToken);

        if (existingTemplate != null)
        {
            throw ValidationException.InvalidInput("Template", "You already have a custom template for this type");
        }

        // Get available variables for this template type
        var variables = await _templateRenderer.GetAvailableVariablesAsync(
            request.TemplateType,
            request.TenantId,
            request.UserId,
            cancellationToken);

        // Find source template (system or tenant level)
        var sourceTemplate = await GetSourceTemplateAsync(
            request.TemplateType,
            request.TenantId,
            request.UserId,
            cancellationToken);

        if (sourceTemplate == null)
        {
            throw NotFoundException.ForEntity("EmailTemplate", request.TemplateType);
        }

        // Create copy
        var newTemplate = new EmailTemplate
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            UserId = request.UserId,
            TemplateType = request.TemplateType,
            Subject = sourceTemplate.Subject,
            HtmlBody = sourceTemplate.HtmlBody,
            PlainTextBody = sourceTemplate.PlainTextBody,
            AvailableVariablesJson = sourceTemplate.AvailableVariablesJson,
            IsActive = true,
            IsSystemDefault = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = _currentUserService.UserId?.ToString()
        };

        _unitOfWork.Repository<EmailTemplate>().Add(newTemplate);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CopyEmailTemplateCommandResponse
        {
            Id = newTemplate.Id,
            Success = true,
            Message = "Template copied successfully for customization"
        };
    }

    private async Task<EmailTemplate?> GetSourceTemplateAsync(
        Meetlr.Domain.Enums.EmailTemplateType templateType,
        Guid? tenantId,
        Guid? userId,
        CancellationToken cancellationToken)
    {
        // If copying to user level, try tenant first, then system
        if (userId.HasValue)
        {
            var tenantTemplate = await _unitOfWork.Repository<EmailTemplate>()
                .GetQueryable()
                .FirstOrDefaultAsync(t =>
                    t.TemplateType == templateType &&
                    t.TenantId == tenantId &&
                    t.UserId == null &&
                    !t.IsDeleted,
                    cancellationToken);

            if (tenantTemplate != null) return tenantTemplate;
        }

        // Get system template
        return await _unitOfWork.Repository<EmailTemplate>()
            .GetQueryable()
            .FirstOrDefaultAsync(t =>
                t.TemplateType == templateType &&
                t.IsSystemDefault &&
                !t.IsDeleted,
                cancellationToken);
    }
}
