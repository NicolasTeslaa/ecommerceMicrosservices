using Order.Domain.Entities;
using Order.Domain.Enums;
using Order.Domain.Exceptions;
using OrderEntity = Order.Domain.Entities.Order;
namespace Order.Tests.Domain;

public class OrderTests
{
    [Fact]
    public void Constructor_ShouldCreatePendingPaymentOrder_WhenDataIsValid()
    {
        var order = new OrderEntity(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "customer@example.com",
            "Rua A, 123",
            25m,
            PaymentMethod.Credit,
            "tok_123",
            "Visa",
            "1234",
            [new OrderItem(Guid.NewGuid(), "Produto", 100m, 2)]);

        Assert.Equal(OrderStatus.PendingPayment, order.Status);
        Assert.Null(order.RejectionReason);
        Assert.Null(order.RejectionDetail);
        Assert.Equal(PaymentMethod.Credit, order.PaymentMethod);
        Assert.Equal("Visa", order.PaymentCardBrand);
        Assert.Equal("1234", order.PaymentCardLast4);
        Assert.Equal(225m, order.TotalAmount);
    }

    [Fact]
    public void Constructor_ShouldTrimPaymentAndAddressFields_WhenValuesContainWhitespace()
    {
        var order = new OrderEntity(
            Guid.NewGuid(),
            Guid.NewGuid(),
            " customer@example.com ",
            " Rua A, 123 ",
            25m,
            PaymentMethod.Credit,
            " tok_123 ",
            " Visa ",
            " 1234 ",
            [new OrderItem(Guid.NewGuid(), "Produto", 100m, 2)]);

        Assert.Equal("customer@example.com", order.CustomerEmail);
        Assert.Equal("Rua A, 123", order.ShippingAddress);
        Assert.Equal("tok_123", order.PaymentToken);
        Assert.Equal("Visa", order.PaymentCardBrand);
        Assert.Equal("1234", order.PaymentCardLast4);
    }

    [Fact]
    public void Constructor_ShouldAllowPixWithoutCardMetadata()
    {
        var order = new OrderEntity(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "customer@example.com",
            "Rua A, 123",
            25m,
            PaymentMethod.Pix,
            null,
            null,
            null,
            [new OrderItem(Guid.NewGuid(), "Produto", 100m, 1)]);

        Assert.Equal(OrderStatus.PendingPayment, order.Status);
        Assert.Equal(PaymentMethod.Pix, order.PaymentMethod);
        Assert.Null(order.PaymentToken);
        Assert.Null(order.PaymentCardBrand);
        Assert.Null(order.PaymentCardLast4);
    }

    [Fact]
    public void Constructor_ShouldThrowInvalidPaymentTokenException_WhenCardPaymentHasNoToken()
    {
        var act = () => new OrderEntity(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "customer@example.com",
            "Rua A, 123",
            25m,
            PaymentMethod.Credit,
            null,
            "Visa",
            "1234",
            [new OrderItem(Guid.NewGuid(), "Produto", 100m, 1)]);

        Assert.Throws<InvalidPaymentTokenException>(act);
    }

    [Fact]
    public void Constructor_ShouldThrowInvalidPaymentCardDataException_WhenCardLast4IsNotNumeric()
    {
        var act = () => new OrderEntity(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "customer@example.com",
            "Rua A, 123",
            25m,
            PaymentMethod.Credit,
            "tok_123",
            "Visa",
            "12A4",
            [new OrderItem(Guid.NewGuid(), "Produto", 100m, 1)]);

        Assert.Throws<InvalidPaymentCardDataException>(act);
    }

    [Fact]
    public void Constructor_ShouldThrowInvalidCustomerIdException_WhenCustomerIdIsEmpty()
    {
        var act = () => new OrderEntity(
            Guid.Empty,
            Guid.NewGuid(),
            "customer@example.com",
            "Rua A, 123",
            25m,
            PaymentMethod.Pix,
            null,
            null,
            null,
            [new OrderItem(Guid.NewGuid(), "Produto", 100m, 1)]);

        Assert.Throws<InvalidCustomerIdException>(act);
    }

    [Fact]
    public void Constructor_ShouldThrowInvalidCustomerAddressIdException_WhenCustomerAddressIdIsEmpty()
    {
        var act = () => new OrderEntity(
            Guid.NewGuid(),
            Guid.Empty,
            "customer@example.com",
            "Rua A, 123",
            25m,
            PaymentMethod.Pix,
            null,
            null,
            null,
            [new OrderItem(Guid.NewGuid(), "Produto", 100m, 1)]);

        Assert.Throws<InvalidCustomerAddressIdException>(act);
    }

    [Fact]
    public void Constructor_ShouldThrowInvalidCustomerEmailException_WhenEmailIsInvalid()
    {
        var act = () => new OrderEntity(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "email-invalido",
            "Rua A, 123",
            25m,
            PaymentMethod.Pix,
            null,
            null,
            null,
            [new OrderItem(Guid.NewGuid(), "Produto", 100m, 1)]);

        Assert.Throws<InvalidCustomerEmailException>(act);
    }

    [Fact]
    public void Constructor_ShouldThrowInvalidShippingAddressException_WhenAddressIsEmpty()
    {
        var act = () => new OrderEntity(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "customer@example.com",
            string.Empty,
            25m,
            PaymentMethod.Pix,
            null,
            null,
            null,
            [new OrderItem(Guid.NewGuid(), "Produto", 100m, 1)]);

        Assert.Throws<InvalidShippingAddressException>(act);
    }

    [Fact]
    public void Constructor_ShouldThrowInvalidShippingAddressException_WhenShippingAmountIsNegative()
    {
        var act = () => new OrderEntity(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "customer@example.com",
            "Rua A, 123",
            -1m,
            PaymentMethod.Pix,
            null,
            null,
            null,
            [new OrderItem(Guid.NewGuid(), "Produto", 100m, 1)]);

        Assert.Throws<InvalidShippingAddressException>(act);
    }

    [Fact]
    public void Constructor_ShouldThrowInvalidPaymentMethodException_WhenPaymentMethodIsUnknown()
    {
        var act = () => new OrderEntity(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "customer@example.com",
            "Rua A, 123",
            25m,
            (PaymentMethod)999,
            null,
            null,
            null,
            [new OrderItem(Guid.NewGuid(), "Produto", 100m, 1)]);

        Assert.Throws<InvalidPaymentMethodException>(act);
    }

    [Fact]
    public void Constructor_ShouldThrowInvalidOrderItemException_WhenItemsAreEmpty()
    {
        var act = () => new OrderEntity(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "customer@example.com",
            "Rua A, 123",
            25m,
            PaymentMethod.Pix,
            null,
            null,
            null,
            []);

        Assert.Throws<InvalidOrderItemException>(act);
    }

    [Fact]
    public void CreateRejected_ShouldThrowInvalidCustomerIdException_WhenCustomerIdIsEmpty()
    {
        var act = () => OrderEntity.CreateRejected(
            Guid.NewGuid(),
            Guid.Empty,
            Guid.NewGuid(),
            10m,
            PaymentMethod.Pix,
            null,
            null,
            null,
            [new OrderItem(Guid.NewGuid(), "Produto", 20m, 1)],
            DateTime.UtcNow,
            OrderRejectionReason.ValidationFailed,
            "erro");

        Assert.Throws<InvalidCustomerIdException>(act);
    }

    [Fact]
    public void CreateRejected_ShouldThrowInvalidCustomerAddressIdException_WhenCustomerAddressIdIsEmpty()
    {
        var act = () => OrderEntity.CreateRejected(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.Empty,
            10m,
            PaymentMethod.Pix,
            null,
            null,
            null,
            [new OrderItem(Guid.NewGuid(), "Produto", 20m, 1)],
            DateTime.UtcNow,
            OrderRejectionReason.ValidationFailed,
            "erro");

        Assert.Throws<InvalidCustomerAddressIdException>(act);
    }

    [Fact]
    public void CreateRejected_ShouldPersistRejectedStatusAndReason()
    {
        var order = OrderEntity.CreateRejected(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            15m,
            PaymentMethod.Pix,
            null,
            null,
            null,
            [new OrderItem(Guid.NewGuid(), "Produto", 50m, 2)],
            DateTime.UtcNow,
            OrderRejectionReason.InsufficientStock,
            "Saldo insuficiente no estoque.");

        Assert.Equal(OrderStatus.PaymentRejected, order.Status);
        Assert.Equal(OrderRejectionReason.InsufficientStock, order.RejectionReason);
        Assert.Equal("Saldo insuficiente no estoque.", order.RejectionDetail);
        Assert.Equal(115m, order.TotalAmount);
    }

    [Fact]
    public void CreateRejected_ShouldUseFallbackValues_WhenOptionalFieldsAreMissing()
    {
        var order = OrderEntity.CreateRejected(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            15m,
            PaymentMethod.Pix,
            null,
            null,
            null,
            [new OrderItem(Guid.NewGuid(), "Produto", 50m, 1)],
            DateTime.UtcNow,
            OrderRejectionReason.ValidationFailed,
            string.Empty);

        Assert.Equal(OrderStatus.PaymentRejected, order.Status);
        Assert.Equal("rejected@order.local", order.CustomerEmail);
        Assert.Equal("Order rejected before confirmation.", order.ShippingAddress);
        Assert.Equal(OrderRejectionReason.ValidationFailed.ToString(), order.RejectionDetail);
    }

    [Fact]
    public void CreateRejected_ShouldThrowInvalidPaymentMethodException_WhenPaymentMethodIsUnknown()
    {
        var act = () => OrderEntity.CreateRejected(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            15m,
            (PaymentMethod)999,
            null,
            null,
            null,
            [new OrderItem(Guid.NewGuid(), "Produto", 50m, 1)],
            DateTime.UtcNow,
            OrderRejectionReason.ValidationFailed,
            "erro");

        Assert.Throws<InvalidPaymentMethodException>(act);
    }

    [Fact]
    public void CreateRejected_ShouldThrowInvalidOrderItemException_WhenItemsAreEmpty()
    {
        var act = () => OrderEntity.CreateRejected(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            15m,
            PaymentMethod.Pix,
            null,
            null,
            null,
            [],
            DateTime.UtcNow,
            OrderRejectionReason.ValidationFailed,
            "erro");

        Assert.Throws<InvalidOrderItemException>(act);
    }
}
