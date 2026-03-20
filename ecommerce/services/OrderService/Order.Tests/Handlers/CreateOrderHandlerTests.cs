using Moq;
using Order.Application.Commands;
using Order.Application.DTOs;
using Order.Application.Handlers;
using Order.Application.Interfaces;
using Order.Domain.Enums;
using Order.Domain.Exceptions;

namespace Order.Tests.Handlers;

public class CreateOrderHandlerTests
{
    private readonly Mock<IOrderCheckoutService> _checkoutServiceMock = new();
    private readonly CreateOrderHandler _handler;

    public CreateOrderHandlerTests()
    {
        _handler = new CreateOrderHandler(_checkoutServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldQueueOrder_WhenCardPaymentHasTokenAndMaskedData()
    {
        var command = CreateCommand();
        var expected = new OrderProcessingAcceptedDto
        {
            OrderId = Guid.NewGuid(),
            Status = "pending_payment",
            Message = "ok",
            RequestedAtUtc = DateTime.UtcNow
        };

        _checkoutServiceMock
            .Setup(service => service.QueueOrderAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.Equal(expected.OrderId, result.OrderId);
        _checkoutServiceMock.Verify(
            service => service.QueueOrderAsync(command, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidPaymentTokenException_WhenCardPaymentHasNoToken()
    {
        var command = CreateCommand();
        command.PaymentToken = null;

        var act = () => _handler.Handle(command, CancellationToken.None);

        await Assert.ThrowsAsync<InvalidPaymentTokenException>(act);
        _checkoutServiceMock.Verify(
            service => service.QueueOrderAsync(It.IsAny<CreateOrderCommand>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidPaymentCardDataException_WhenMaskedDataIsMissing()
    {
        var command = CreateCommand();
        command.PaymentCardBrand = null;

        var act = () => _handler.Handle(command, CancellationToken.None);

        await Assert.ThrowsAsync<InvalidPaymentCardDataException>(act);
        _checkoutServiceMock.Verify(
            service => service.QueueOrderAsync(It.IsAny<CreateOrderCommand>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldQueueOrder_WhenPaymentMethodIsPixWithoutCardMetadata()
    {
        var command = CreateCommand();
        command.PaymentMethod = PaymentMethod.Pix;
        command.PaymentToken = null;
        command.PaymentCardBrand = null;
        command.PaymentCardLast4 = null;

        _checkoutServiceMock
            .Setup(service => service.QueueOrderAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrderProcessingAcceptedDto
            {
                OrderId = Guid.NewGuid(),
                Status = "pending_payment",
                Message = "ok",
                RequestedAtUtc = DateTime.UtcNow
            });

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.Equal("pending_payment", result.Status);
        _checkoutServiceMock.Verify(
            service => service.QueueOrderAsync(command, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidCustomerAddressIdException_WhenCustomerAddressIsEmpty()
    {
        var command = CreateCommand();
        command.CustomerAddressId = Guid.Empty;

        var act = () => _handler.Handle(command, CancellationToken.None);

        await Assert.ThrowsAsync<InvalidCustomerAddressIdException>(act);
        _checkoutServiceMock.Verify(
            service => service.QueueOrderAsync(It.IsAny<CreateOrderCommand>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldQueueOrder_WhenPaymentMethodIsDebitAndMaskedDataIsPresent()
    {
        var command = CreateCommand();
        command.PaymentMethod = PaymentMethod.Debit;

        _checkoutServiceMock
            .Setup(service => service.QueueOrderAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrderProcessingAcceptedDto
            {
                OrderId = Guid.NewGuid(),
                Status = "pending_payment",
                Message = "ok",
                RequestedAtUtc = DateTime.UtcNow
            });

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.Equal("pending_payment", result.Status);
        _checkoutServiceMock.Verify(
            service => service.QueueOrderAsync(command, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidPaymentCardDataException_WhenCardLast4IsMissing()
    {
        var command = CreateCommand();
        command.PaymentCardLast4 = null;

        var act = () => _handler.Handle(command, CancellationToken.None);

        await Assert.ThrowsAsync<InvalidPaymentCardDataException>(act);
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidPaymentCardDataException_WhenDebitPaymentHasNoBrand()
    {
        var command = CreateCommand();
        command.PaymentMethod = PaymentMethod.Debit;
        command.PaymentCardBrand = string.Empty;

        var act = () => _handler.Handle(command, CancellationToken.None);

        await Assert.ThrowsAsync<InvalidPaymentCardDataException>(act);
    }

    private static CreateOrderCommand CreateCommand()
    {
        return new CreateOrderCommand
        {
            CustomerId = Guid.NewGuid(),
            CustomerAddressId = Guid.NewGuid(),
            ShippingAmount = 20m,
            PaymentMethod = PaymentMethod.Credit,
            PaymentToken = "tok_123",
            PaymentCardBrand = "Visa",
            PaymentCardLast4 = "1234",
            Items =
            [
                new CreateOrderItemRequest
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Produto",
                    UnitPrice = 150m,
                    Quantity = 1
                }
            ]
        };
    }
}
