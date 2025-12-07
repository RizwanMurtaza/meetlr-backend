using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Interfaces;
using Meetlr.Domain.Entities.Emails;
using Meetlr.Domain.Exceptions.Http;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Application.Features.EmailTemplates.Commands.UpdateEmailTemplate;

public class UpdateEmailTemplateCommandHandler : IRequestHandler<UpdateEmailTemplateCommand, UpdateEmailTemplateCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public UpdateEmailTemplateCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<UpdateEmailTemplateCommandResponse> Handle(UpdateEmailTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = await _unitOfWork.Repository<EmailTemplate>()
            .GetQueryable()
            .FirstOrDefaultAsync(t => t.Id == request.Id && !t.IsDeleted, cancellationToken);

        if (template == null)
        {
            throw NotFoundException.ForEntity("EmailTemplate", request.Id);
        }

        // Prevent editing system default templates
        if (template.IsSystemDefault)
        {
            throw ValidationException.InvalidInput("Template", "System default templates cannot be modified");
        }

        // Update template
        template.Subject = request.Subject;
        template.HtmlBody = request.HtmlBody;
        template.PlainTextBody = request.PlainTextBody;
        template.IsActive = request.IsActive;
        template.UpdatedAt = DateTime.UtcNow;
        template.UpdatedBy = _currentUserService.UserId?.ToString();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new UpdateEmailTemplateCommandResponse
        {
            Id = template.Id,
            Success = true,
            Message = "Email template updated successfully"
        };
    }
}
