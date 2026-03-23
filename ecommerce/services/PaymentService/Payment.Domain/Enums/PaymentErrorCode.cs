namespace Payment.Domain.Enums;

public enum PaymentErrorCode
{
    Unknown = 0,
    InvalidOrderId = 1,
    InvalidCustomerId = 2,
    InvalidAmount = 3,
    InvalidCurrency = 4,
    InvalidPaymentMethod = 5,
    InvalidPaymentIntent = 6,
    PaymentNotFound = 7,
    PersistenceError = 8,
    InvalidRequest = 9
}
