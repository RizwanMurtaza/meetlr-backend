using MediatR;

namespace Meetlr.Application.Features.Payments.Commands.ProcessRefund;

public class ProcessRefundCommand : IRequest<ProcessRefundCommandResponse>
{
    public Guid NotificationPendingId { get; set; }
}
