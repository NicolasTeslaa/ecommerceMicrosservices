using Moq;
using Order.Application.DTOs;
using Order.Application.Handlers;
using Order.Application.Interfaces;
using Order.Application.Queries;
using Order.Application.ReadModels;
using Order.Domain.Enums;
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
    public async Task Handle_ShouldReturnFallbackOrder_WhenOrderIdIsEmpty()
    {
        var result = await _handler.Handle(new GetOrderByIdQuery(Guid.Empty), CancellationToken.None);

        Assert.Equal(Guid.Empty, result.Id);
        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task Handle_ShouldReturnFallbackOrder_WhenRepositoryReturnsNull()
    {
        var orderId = Guid.NewGuid();

        _repositoryMock
            .Setup(repository => repository.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrderReadModel?)null);

        var result = await _handler.Handle(new GetOrderByIdQuery(orderId), CancellationToken.None);

        Assert.Equal(orderId, result.Id);
        Assert.Empty(result.Items);
        Assert.Equal("Order not available.", result.RejectionDetail);
    }
}
