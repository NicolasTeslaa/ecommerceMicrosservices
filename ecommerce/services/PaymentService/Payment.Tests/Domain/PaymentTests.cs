using Payment.Domain.Enums;
using Payment.Domain.Exceptions;
using Payment.Tests.Support;

namespace Payment.Tests.Domain;

public class PaymentTests
{
    [Fact]
    public void Constructor_ShouldCreatePendingPayment_WhenDataIsValid()
    {
        var payment = PaymentTestData.CreatePayment();

        Assert.Equal(PaymentStatus.Pending, payment.Status);
        Assert.Equal("brl", payment.Currency);
        Assert.Equal(0, payment.AttemptCount);
        Assert.False(payment.HasReachedMaxAttempts);
    }

    [Fact]
    public void Constructor_ShouldThrowInvalidOrderIdException_WhenOrderIdIsEmpty()
    {
        var act = () => new Payment.Domain.Entities.Payment(Guid.Empty, Guid.NewGuid(), 100m, "BRL", PaymentMethod.Card);

        Assert.Throws<InvalidOrderIdException>(act);
    }

    [Fact]
    public void Constructor_ShouldThrowInvalidCustomerIdException_WhenCustomerIdIsEmpty()
    {
        var act = () => new Payment.Domain.Entities.Payment(Guid.NewGuid(), Guid.Empty, 100m, "BRL", PaymentMethod.Card);

        Assert.Throws<InvalidCustomerIdException>(act);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_ShouldThrowInvalidAmountException_WhenAmountIsZero(decimal amount)
    {
        var act = () => new Payment.Domain.Entities.Payment(Guid.NewGuid(), Guid.NewGuid(), amount, "BRL", PaymentMethod.Card);

        Assert.Throws<InvalidAmountException>(act);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("br")]
    [InlineData("brll")]
    public void Constructor_ShouldThrowInvalidCurrencyException_WhenCurrencyIsInvalid(string currency)
    {
        var act = () => new Payment.Domain.Entities.Payment(Guid.NewGuid(), Guid.NewGuid(), 10m, currency, PaymentMethod.Card);

        Assert.Throws<InvalidCurrencyException>(act);
    }

    [Fact]
    public void Constructor_ShouldThrowInvalidPaymentMethodException_WhenPaymentMethodIsInvalid()
    {
        var act = () => new Payment.Domain.Entities.Payment(Guid.NewGuid(), Guid.NewGuid(), 10m, "BRL", (PaymentMethod)999);

        Assert.Throws<InvalidPaymentMethodException>(act);
    }

    [Fact]
    public void SetPaymentIntent_ShouldMovePaymentToPendingConfirmation()
    {
        var payment = PaymentTestData.CreatePayment();

        payment.SetPaymentIntent(" pi_123 ", " secret_123 ", " pm_123 ");

        Assert.Equal(PaymentStatus.PendingConfirmation, payment.Status);
        Assert.Equal("pi_123", payment.StripePaymentIntentId);
        Assert.Equal("secret_123", payment.StripeClientSecret);
        Assert.Equal("pm_123", payment.StripePaymentMethodId);
    }

    [Fact]
    public void SetPaymentIntent_ShouldAllowNullPaymentMethodId()
    {
        var payment = PaymentTestData.CreatePayment();

        payment.SetPaymentIntent("pi_123", "secret_123");

        Assert.Null(payment.StripePaymentMethodId);
    }

    [Theory]
    [InlineData("", "secret")]
    [InlineData("pi_123", "")]
    public void SetPaymentIntent_ShouldThrowInvalidPaymentIntentException_WhenDataIsMissing(string paymentIntentId, string clientSecret)
    {
        var payment = PaymentTestData.CreatePayment();

        var act = () => payment.SetPaymentIntent(paymentIntentId, clientSecret);

        Assert.Throws<InvalidPaymentIntentException>(act);
    }

    [Fact]
    public void MarkRequiresAction_ShouldSetStatusReasonAndDetail()
    {
        var payment = PaymentTestData.CreatePaymentWithIntent();

        payment.MarkRequiresAction(PaymentFailureReason.RequiresCustomerAction, " autenticar ");

        Assert.Equal(PaymentStatus.RequiresAction, payment.Status);
        Assert.Equal(PaymentFailureReason.RequiresCustomerAction, payment.FailureReason);
        Assert.Equal("autenticar", payment.FailureDetail);
    }

    [Fact]
    public void MarkApproved_ShouldClearFailureData()
    {
        var payment = PaymentTestData.CreatePaymentWithIntent();
        payment.MarkFailed(PaymentFailureReason.CardDeclined, "erro");

        payment.MarkApproved();

        Assert.Equal(PaymentStatus.Approved, payment.Status);
        Assert.Null(payment.FailureReason);
        Assert.Null(payment.FailureDetail);
    }

    [Fact]
    public void MarkFailed_ShouldStoreFailureReasonAndDetail()
    {
        var payment = PaymentTestData.CreatePayment();

        payment.MarkFailed(PaymentFailureReason.CardDeclined, "Card declined.");

        Assert.Equal(PaymentStatus.Failed, payment.Status);
        Assert.Equal(PaymentFailureReason.CardDeclined, payment.FailureReason);
        Assert.Equal("Card declined.", payment.FailureDetail);
        Assert.Equal(1, payment.AttemptCount);
    }

    [Fact]
    public void MarkFailed_ShouldUseReasonName_WhenDetailIsBlank()
    {
        var payment = PaymentTestData.CreatePayment();

        payment.MarkFailed(PaymentFailureReason.ProcessorError, " ");

        Assert.Equal("ProcessorError", payment.FailureDetail);
    }

    [Fact]
    public void MarkFailed_ShouldIncrementAttemptsUntilMaxAttemptsReached()
    {
        var payment = PaymentTestData.CreatePayment();

        payment.MarkFailed(PaymentFailureReason.CardDeclined, "Falha 1.");
        payment.MarkFailed(PaymentFailureReason.CardDeclined, "Falha 2.");
        payment.MarkFailed(PaymentFailureReason.CardDeclined, "Falha 3.");

        Assert.Equal(3, payment.AttemptCount);
        Assert.True(payment.HasReachedMaxAttempts);
    }

    [Fact]
    public void MarkCancelled_ShouldSetCancelledStatus_AndTrimDetail()
    {
        var payment = PaymentTestData.CreatePayment();

        payment.MarkCancelled(" cancelado ");

        Assert.Equal(PaymentStatus.Cancelled, payment.Status);
        Assert.Equal("cancelado", payment.FailureDetail);
    }

    [Fact]
    public void MarkCancelled_ShouldAllowNullDetail()
    {
        var payment = PaymentTestData.CreatePayment();

        payment.MarkCancelled();

        Assert.Null(payment.FailureDetail);
    }

    [Fact]
    public void StateChangingMethods_ShouldUpdateTimestamp()
    {
        var payment = PaymentTestData.CreatePayment();
        var originalUpdatedAt = payment.UpdatedAtUtc;

        Thread.Sleep(10);
        payment.SetPaymentIntent("pi_123", "secret_123");

        Assert.True(payment.UpdatedAtUtc > originalUpdatedAt);
    }
}
