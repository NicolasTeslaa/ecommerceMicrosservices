namespace ECommerce.Shared.Messaging;

public class PaymentFailedIntegrationEvent
{
    public Guid PaymentId { get; set; }
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string? StripePaymentIntentId { get; set; }
    public string FailureReason { get; set; } = string.Empty;
    public string FailureDetail { get; set; } = string.Empty;
    public int AttemptCount { get; set; }
    public bool MaxAttemptsReached { get; set; }
    public DateTime FailedAtUtc { get; set; }
}
