namespace Payment.Domain.Enums;

public enum PaymentStatus
{
    Pending = 1,
    PendingConfirmation = 2,
    RequiresAction = 3,
    Approved = 4,
    Failed = 5,
    Cancelled = 6
}
