using Payment.Domain.Enums;

namespace Payment.Tests.Support;

internal static class PaymentTestData
{
    public static Payment.Domain.Entities.Payment CreatePayment(
        PaymentMethod paymentMethod = PaymentMethod.Card,
        decimal amount = 125.50m,
        string currency = "brl")
    {
        return new Payment.Domain.Entities.Payment(
            Guid.NewGuid(),
            Guid.NewGuid(),
            amount,
            currency,
            paymentMethod);
    }

    public static Payment.Domain.Entities.Payment CreatePaymentWithIntent(
        PaymentMethod paymentMethod = PaymentMethod.Card,
        decimal amount = 125.50m,
        string currency = "brl")
    {
        var payment = CreatePayment(paymentMethod, amount, currency);
        payment.SetPaymentIntent("pi_test_123", "secret_test_123", "pm_test_123");
        return payment;
    }
}
