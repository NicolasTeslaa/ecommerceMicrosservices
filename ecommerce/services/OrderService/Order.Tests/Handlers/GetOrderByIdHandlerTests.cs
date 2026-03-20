using Moq;
using Order.Application.DTOs;
using Order.Application.Handlers;
using Order.Application.Interfaces;
using Order.Application.Queries;
using Order.Application.ReadModels;
using Order.Domain.Enums;
using Order.Domain.Exceptions;
using Order.Tests.Support;

namespace Order.Tests.Handlers;

public class GetOrderByIdHandlerTests
{
    private readonly Mock<IOrderReadRepository> _repositoryMock = new();
    private readonly GetOrderByIdHandler _handler;

    public GetOrderByIdHandlerTests()
    {
        _handler = new GetOrderByIdHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnMappedOrder_WhenOrderExists()
    {
        var readModel = OrderTestData.CreateReadModel();

        _repositoryMock
            .Setup(repository => repository.GetByIdAsync(readModel.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(readModel);

        var result = await _handler.Handle(new GetOrderByIdQuery(readModel.Id), CancellationToken.None);

        Assert.Equal(readModel.Id, result.Id);
        Assert.Equal(readModel.CustomerEmail, result.CustomerEmail);
        Assert.Equal(readModel.PaymentMethod, result.PaymentMethod);
        Assert.Single(result.Items);
    }

    [Fact]
    public async Task Handle_ShouldReturnRejectionData_WhenRejectedOrderExists()
    {
        var readModel = OrderTestData.CreateReadModel(
            status: OrderStatus.PaymentRejected,
            rejectionReason: OrderRejectionReason.ProductUnavailable);

        _repositoryMock
            .Setup(repository => repository.GetByIdAsync(readModel.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(readModel);

        var result = await _handler.Handle(new GetOrderByIdQuery(readModel.Id), CancellationToken.None);

        Assert.Equal(OrderStatus.PaymentRejected, result.Status);
        Assert.Equal(OrderRejectionReason.ProductUnavailable, result.RejectionReason);
        Assert.Equal("Pedido rejeitado.", result.RejectionDetail);
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidOrderIdException_WhenOrderIdIsEmpty()
    {
        var act = () => _handler.Handle(new GetOrderByIdQuery(Guid.Empty), CancellationToken.None);

        await Assert.ThrowsAsync<InvalidOrderIdException>(act);
    }

    [Fact]
    public async Task Handle_ShouldThrowOrderNotFoundException_WhenRepositoryReturnsNull()
    {
        var orderId = Guid.NewGuid();

        _repositoryMock
            .Setup(repository => repository.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrderReadModel?)null);

        var act = () => _handler.Handle(new GetOrderByIdQuery(orderId), CancellationToken.None);

        await Assert.ThrowsAsync<OrderNotFoundException>(act);
    }
}
