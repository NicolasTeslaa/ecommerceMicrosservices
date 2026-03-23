using Payment.Domain.Enums;

namespace Payment.Application.DTOs;

public class PaymentDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public PaymentMethod PaymentMethod { get; set; }
    public string? StripePaymentIntentId { get; set; }
    public string? StripeClientSecret { get; set; }
    public PaymentStatus Status { get; set; }
    public PaymentFailureReason? FailureReason { get; set; }
    public string? FailureDetail { get; set; }
    public int AttemptCount { get; set; }
    public bool MaxAttemptsReached { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}
