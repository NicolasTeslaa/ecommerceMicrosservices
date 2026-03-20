namespace Order.Domain.Enums;

public enum OrderRejectionReason
{
    None = 0,
    ProductUnavailable = 1,
    InsufficientStock = 2,
    InvalidCustomerAddress = 3,
    ValidationFailed = 4
}
