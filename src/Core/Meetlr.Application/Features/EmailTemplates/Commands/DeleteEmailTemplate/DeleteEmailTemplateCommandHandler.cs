using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Application.Interfaces;
using Meetlr.Domain.Entities.Emails;
using Meetlr.Domain.Exceptions.Http;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Application.Features.EmailTemplates.Commands.DeleteEmailTemplate;

public class DeleteEmailTemplateCommandHandler : IRequestHandler<DeleteEmailTemplateCommand, DeleteEmailTemplateCommandResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public DeleteEmailTemplateCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<DeleteEmailTemplateCommandResponse> Handle(DeleteEmailTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = await _unitOfWork.Repository<EmailTemplate>()
            .GetQueryable()
            .FirstOrDefaultAsync(t => t.Id == request.Id && !t.IsDeleted, cancellationToken);

        if (template == null)
        {
            throw NotFoundException.ForEntity("EmailTemplate", request.Id);
        }

        // Prevent deleting system templates
        if (template.IsSystemDefault)
        {
            throw ValidationException.InvalidInput("Template", "System default templates cannot be deleted");
        }

        // Soft delete
        template.IsDeleted = true;
        template.DeletedAt = DateTime.UtcNow;
        template.DeletedBy = _currentUserService.UserId?.ToString();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new DeleteEmailTemplateCommandResponse
        {
            Success = true,
            Message = "Template deleted successfully. Will revert to parent template."
        };
    }
}
