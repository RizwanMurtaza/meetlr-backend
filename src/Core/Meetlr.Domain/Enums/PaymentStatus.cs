namespace Meetlr.Domain.Enums;

public enum PaymentStatus
{
    NotRequired = 0,
    Pending = 1,
    Initiated = 2,
    Completed = 3,
    Failed = 4,
    Refunded = 5,
    RefundPending = 6
}
