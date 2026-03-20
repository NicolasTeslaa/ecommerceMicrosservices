namespace Order.Domain.Enums;

public enum OrderStatus
{
    PendingPayment = 1,
    Pending = PendingPayment,
    Confirmed = 2,
    Cancelled = 3,
    PaymentRejected = 4
}
