using MediatR;
using Meetlr.Application.Common.Interfaces;
using Meetlr.Domain.Entities.Emails;
using Microsoft.EntityFrameworkCore;

namespace Meetlr.Application.Features.MeetlrEvents.Commands.DeleteEmailTemplate;

public class DeleteEventEmailTemplateCommandHandler : IRequestHandler<DeleteEventEmailTemplateCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteEventEmailTemplateCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteEventEmailTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = await _unitOfWork.Repository<EventEmailTemplate>()
            .GetQueryable()
            .FirstOrDefaultAsync(t =>
                t.MeetlrEventId == request.MeetlrEventId &&
                t.TemplateType == request.TemplateType &&
                !t.IsDeleted,
                cancellationToken);

        if (template == null)
        {
            return false;
        }

        // Soft delete
        template.IsDeleted = true;
        template.DeletedAt = DateTime.UtcNow;

        _unitOfWork.Repository<EventEmailTemplate>().Update(template);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
