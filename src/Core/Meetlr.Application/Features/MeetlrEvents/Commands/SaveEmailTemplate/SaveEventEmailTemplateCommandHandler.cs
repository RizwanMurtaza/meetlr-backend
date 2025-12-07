using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Domain.Entities.Emails;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Application.Features.MeetlrEvents.Commands.SaveEmailTemplate;

public class SaveEventEmailTemplateCommandHandler : IRequestHandler<SaveEventEmailTemplateCommand, SaveEventEmailTemplateResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public SaveEventEmailTemplateCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<SaveEventEmailTemplateResponse> Handle(SaveEventEmailTemplateCommand request, CancellationToken cancellationToken)
    {
        // Check if template already exists for this event and type
        // Include soft-deleted templates since MySQL doesn't support filtered unique indexes
        var existingTemplate = await _unitOfWork.Repository<EventEmailTemplate>()
            .GetQueryable()
            .IgnoreQueryFilters() // Include soft-deleted records
            .FirstOrDefaultAsync(t =>
                t.MeetlrEventId == request.MeetlrEventId &&
                t.TemplateType == request.TemplateType,
                cancellationToken);

        if (existingTemplate != null)
        {
            // Update existing (or restore if soft-deleted)
            existingTemplate.Subject = request.Subject;
            existingTemplate.HtmlBody = request.HtmlBody;
            existingTemplate.IsActive = request.IsActive;
            existingTemplate.IsDeleted = false; // Restore if soft-deleted
            existingTemplate.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<EventEmailTemplate>().Update(existingTemplate);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new SaveEventEmailTemplateResponse
            {
                Id = existingTemplate.Id,
                Success = true,
                Message = "Template updated successfully"
            };
        }
        else
        {
            // Create new
            var newTemplate = new EventEmailTemplate
            {
                Id = Guid.NewGuid(),
                MeetlrEventId = request.MeetlrEventId,
                TemplateType = request.TemplateType,
                Subject = request.Subject,
                HtmlBody = request.HtmlBody,
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _unitOfWork.Repository<EventEmailTemplate>().Add(newTemplate);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new SaveEventEmailTemplateResponse
            {
                Id = newTemplate.Id,
                Success = true,
                Message = "Template created successfully"
            };
        }
    }
}
