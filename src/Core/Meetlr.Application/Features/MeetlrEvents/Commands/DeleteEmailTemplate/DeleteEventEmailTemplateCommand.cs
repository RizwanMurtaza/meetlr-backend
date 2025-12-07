using MediatR;
using Meetlr.Domain.Enums;

namespace Meetlr.Application.Features.MeetlrEvents.Commands.DeleteEmailTemplate;

public class DeleteEventEmailTemplateCommand : IRequest<bool>
{
    public Guid MeetlrEventId { get; set; }
    public EmailTemplateType TemplateType { get; set; }
}
