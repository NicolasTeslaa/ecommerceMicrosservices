using Moq;
using Payment.Application.Handlers;
using Payment.Application.Interfaces;
using Payment.Application.Queries;
using Payment.Domain.Exceptions;
using Payment.Tests.Support;

namespace Payment.Tests.Handlers;

public class GetPaymentByOrderIdHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnPaymentDto_WhenPaymentExists()
    {
        var repositoryMock = new Mock<IPaymentRepository>();
        var payment = PaymentTestData.CreatePaymentWithIntent();
        var handler = new GetPaymentByOrderIdHandler(repositoryMock.Object);

        repositoryMock
            .Setup(repository => repository.GetByOrderIdAsync(payment.OrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        var result = await handler.Handle(new GetPaymentByOrderIdQuery(payment.OrderId), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(payment.OrderId, result!.OrderId);
        Assert.Equal(payment.Amount, result.Amount);
        Assert.Equal(payment.StripeClientSecret, result.StripeClientSecret);
        Assert.Equal(payment.HasReachedMaxAttempts, result.MaxAttemptsReached);
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenPaymentDoesNotExist()
    {
        var repositoryMock = new Mock<IPaymentRepository>();
        var orderId = Guid.NewGuid();
        var handler = new GetPaymentByOrderIdHandler(repositoryMock.Object);

        repositoryMock
            .Setup(repository => repository.GetByOrderIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment.Domain.Entities.Payment?)null);

        var result = await handler.Handle(new GetPaymentByOrderIdQuery(orderId), CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOrderIdException_WhenOrderIdIsEmpty()
    {
        var repositoryMock = new Mock<IPaymentRepository>();
        var handler = new GetPaymentByOrderIdHandler(repositoryMock.Object);

        await Assert.ThrowsAsync<InvalidOrderIdException>(() => handler.Handle(new GetPaymentByOrderIdQuery(Guid.Empty), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldCallRepositoryWithRequestedOrderId()
    {
        var repositoryMock = new Mock<IPaymentRepository>();
        var orderId = Guid.NewGuid();
        var handler = new GetPaymentByOrderIdHandler(repositoryMock.Object);

        await handler.Handle(new GetPaymentByOrderIdQuery(orderId), CancellationToken.None);

        repositoryMock.Verify(repository => repository.GetByOrderIdAsync(orderId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
