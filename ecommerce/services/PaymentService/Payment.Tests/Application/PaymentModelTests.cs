using Payment.Application.DTOs;
using Payment.Application.Models;
using Payment.Application.Queries;
using Payment.Domain.Enums;

namespace Payment.Tests.Application;

public class PaymentModelTests
{
    [Fact]
    public void GetPaymentByOrderIdQuery_ShouldStoreOrderId()
    {
        var orderId = Guid.NewGuid();

        var query = new GetPaymentByOrderIdQuery(orderId);

        Assert.Equal(orderId, query.OrderId);
    }

    [Fact]
    public void StripePaymentIntentResult_ShouldStoreAssignedValues()
    {
        var result = new StripePaymentIntentResult
        {
            PaymentIntentId = "pi_123",
            ClientSecret = "secret_123",
            Status = "requires_payment_method",
            PaymentMethodId = "pm_123"
        };

        Assert.Equal("pi_123", result.PaymentIntentId);
        Assert.Equal("secret_123", result.ClientSecret);
        Assert.Equal("requires_payment_method", result.Status);
        Assert.Equal("pm_123", result.PaymentMethodId);
    }

    [Fact]
    public void PaymentDto_ShouldAllowPropertyAssignment()
    {
        var dto = new PaymentDto
        {
            Id = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            Amount = 44.10m,
            Currency = "brl",
            PaymentMethod = PaymentMethod.Card,
            StripePaymentIntentId = "pi_1",
            StripeClientSecret = "secret_1",
            Status = PaymentStatus.PendingConfirmation,
            FailureReason = PaymentFailureReason.CardDeclined,
            FailureDetail = "erro",
            AttemptCount = 2,
            MaxAttemptsReached = false,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        Assert.Equal("brl", dto.Currency);
        Assert.Equal(PaymentMethod.Card, dto.PaymentMethod);
        Assert.Equal(PaymentStatus.PendingConfirmation, dto.Status);
        Assert.Equal(2, dto.AttemptCount);
    }
}
