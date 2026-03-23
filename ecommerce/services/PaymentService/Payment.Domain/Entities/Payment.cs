using Payment.Domain.Enums;
using Payment.Domain.Exceptions;

namespace Payment.Domain.Entities;

public class Payment
{
    public const int MaxAttemptsAllowed = 3;

    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public Guid CustomerId { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public PaymentMethod PaymentMethod { get; private set; }
    public string? StripePaymentIntentId { get; private set; }
    public string? StripeClientSecret { get; private set; }
    public string? StripePaymentMethodId { get; private set; }
    public PaymentStatus Status { get; private set; }
    public PaymentFailureReason? FailureReason { get; private set; }
    public string? FailureDetail { get; private set; }
    public int AttemptCount { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public bool HasReachedMaxAttempts => AttemptCount >= MaxAttemptsAllowed;

    private Payment()
    {
    }

    public Payment(Guid orderId, Guid customerId, decimal amount, string currency, PaymentMethod paymentMethod)
    {
        Validate(orderId, customerId, amount, currency, paymentMethod);

        Id = Guid.NewGuid();
        OrderId = orderId;
        CustomerId = customerId;
        Amount = amount;
        Currency = currency.Trim().ToLowerInvariant();
        PaymentMethod = paymentMethod;
        Status = PaymentStatus.Pending;
        AttemptCount = 0;
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = CreatedAtUtc;
    }

    public void SetPaymentIntent(string paymentIntentId, string clientSecret, string? paymentMethodId = null)
    {
        if (string.IsNullOrWhiteSpace(paymentIntentId) || string.IsNullOrWhiteSpace(clientSecret))
            throw new InvalidPaymentIntentException();

        StripePaymentIntentId = paymentIntentId.Trim();
        StripeClientSecret = clientSecret.Trim();
        StripePaymentMethodId = string.IsNullOrWhiteSpace(paymentMethodId) ? null : paymentMethodId.Trim();
        Status = PaymentStatus.PendingConfirmation;
        Touch();
    }

    public void MarkRequiresAction(PaymentFailureReason failureReason, string? detail = null)
    {
        Status = PaymentStatus.RequiresAction;
        FailureReason = failureReason;
        FailureDetail = string.IsNullOrWhiteSpace(detail) ? null : detail.Trim();
        Touch();
    }

    public void MarkApproved()
    {
        Status = PaymentStatus.Approved;
        FailureReason = null;
        FailureDetail = null;
        Touch();
    }

    public void MarkFailed(PaymentFailureReason failureReason, string detail)
    {
        AttemptCount += 1;
        Status = PaymentStatus.Failed;
        FailureReason = failureReason;
        FailureDetail = string.IsNullOrWhiteSpace(detail) ? failureReason.ToString() : detail.Trim();
        Touch();
    }

    public void MarkCancelled(string? detail = null)
    {
        Status = PaymentStatus.Cancelled;
        FailureDetail = string.IsNullOrWhiteSpace(detail) ? null : detail.Trim();
        Touch();
    }

    private static void Validate(Guid orderId, Guid customerId, decimal amount, string currency, PaymentMethod paymentMethod)
    {
        if (orderId == Guid.Empty)
            throw new InvalidOrderIdException();
        if (customerId == Guid.Empty)
            throw new InvalidCustomerIdException();
        if (amount <= 0)
            throw new InvalidAmountException();
        if (string.IsNullOrWhiteSpace(currency) || currency.Trim().Length != 3)
            throw new InvalidCurrencyException();
        if (!Enum.IsDefined(paymentMethod))
            throw new InvalidPaymentMethodException();
    }

    private void Touch()
    {
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
