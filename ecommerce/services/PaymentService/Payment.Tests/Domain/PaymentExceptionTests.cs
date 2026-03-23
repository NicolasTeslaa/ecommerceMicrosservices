using Payment.Domain.Enums;
using Payment.Domain.Exceptions;

namespace Payment.Tests.Domain;

public class PaymentExceptionTests
{
    [Fact]
    public void InvalidAmountException_ShouldExposeExpectedErrorCode()
    {
        var exception = new InvalidAmountException();

        Assert.Equal(PaymentErrorCode.InvalidAmount, exception.ErrorCode);
        Assert.Equal("Payment amount must be greater than zero.", exception.Message);
    }

    [Fact]
    public void InvalidCurrencyException_ShouldExposeExpectedErrorCode()
    {
        var exception = new InvalidCurrencyException();

        Assert.Equal(PaymentErrorCode.InvalidCurrency, exception.ErrorCode);
    }

    [Fact]
    public void InvalidCustomerIdException_ShouldExposeExpectedErrorCode()
    {
        var exception = new InvalidCustomerIdException();

        Assert.Equal(PaymentErrorCode.InvalidCustomerId, exception.ErrorCode);
    }

    [Fact]
    public void InvalidOrderIdException_ShouldExposeExpectedErrorCode()
    {
        var exception = new InvalidOrderIdException();

        Assert.Equal(PaymentErrorCode.InvalidOrderId, exception.ErrorCode);
    }

    [Fact]
    public void InvalidPaymentIntentException_ShouldExposeExpectedErrorCode()
    {
        var exception = new InvalidPaymentIntentException();

        Assert.Equal(PaymentErrorCode.InvalidPaymentIntent, exception.ErrorCode);
    }

    [Fact]
    public void InvalidPaymentMethodException_ShouldExposeExpectedErrorCode()
    {
        var exception = new InvalidPaymentMethodException();

        Assert.Equal(PaymentErrorCode.InvalidPaymentMethod, exception.ErrorCode);
    }

    [Fact]
    public void PaymentNotFoundException_ShouldContainOrderId()
    {
        var orderId = Guid.NewGuid();
        var exception = new PaymentNotFoundException(orderId);

        Assert.Equal(PaymentErrorCode.PaymentNotFound, exception.ErrorCode);
        Assert.Contains(orderId.ToString(), exception.Message);
    }

    [Fact]
    public void PersistenceException_ShouldStoreInnerExceptionInData()
    {
        var inner = new InvalidOperationException("db");
        var exception = new PersistenceException("failure", inner);

        Assert.Equal(PaymentErrorCode.PersistenceError, exception.ErrorCode);
        Assert.Equal(inner, exception.Data["InnerException"]);
    }

    [Fact]
    public void PersistenceException_ShouldWorkWithoutInnerException()
    {
        var exception = new PersistenceException("failure");

        Assert.False(exception.Data.Contains("InnerException"));
    }
}
