namespace Payment.Domain.Enums;

public enum PaymentFailureReason
{
    None = 0,
    InvalidPaymentMethod = 1,
    CardDeclined = 2,
    RequiresCustomerAction = 3,
    ProcessorError = 4,
    WebhookValidationFailed = 5,
    Unknown = 6
}
