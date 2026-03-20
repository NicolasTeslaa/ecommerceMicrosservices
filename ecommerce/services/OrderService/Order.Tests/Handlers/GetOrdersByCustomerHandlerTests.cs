using ECommerce.Shared.Contracts;
using Moq;
using Order.Application.Handlers;
using Order.Application.Interfaces;
using Order.Application.Queries;
using Order.Application.ReadModels;
using Order.Domain.Enums;
using Order.Domain.Exceptions;
using Order.Tests.Support;

namespace Order.Tests.Handlers;

public class GetOrdersByCustomerHandlerTests
{
    private readonly Mock<IOrderReadRepository> _repositoryMock = new();
    private readonly GetOrdersByCustomerHandler _handler;

    public GetOrdersByCustomerHandlerTests()
    {
        _handler = new GetOrdersByCustomerHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnMappedPagedOrders_WhenRepositoryReturnsItems()
    {
        var customerId = Guid.NewGuid();
        var paged = PagedResult<OrderReadModel>.Create(
            [OrderTestData.CreateReadModel(), OrderTestData.CreateReadModel(paymentMethod: PaymentMethod.Pix)],
            1,
            10,
            2);

        _repositoryMock
            .Setup(repository => repository.GetByCustomerIdAsync(
                customerId,
                It.IsAny<GetOrdersByCustomerQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(paged);

        var result = await _handler.Handle(new GetOrdersByCustomerQuery { CustomerId = customerId }, CancellationToken.None);

        Assert.Equal(2, result.Items.Count);
        Assert.Equal(2, result.Pagination.TotalItems);
    }

    [Fact]
    public async Task Handle_ShouldReturnRejectedOrders_WhenRepositoryContainsRejectedItems()
    {
        var customerId = Guid.NewGuid();
        var rejectedReadModel = OrderTestData.CreateReadModel(
            status: OrderStatus.PaymentRejected,
            rejectionReason: OrderRejectionReason.InvalidCustomerAddress);

        _repositoryMock
            .Setup(repository => repository.GetByCustomerIdAsync(
                customerId,
                It.IsAny<GetOrdersByCustomerQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(PagedResult<OrderReadModel>.Create([rejectedReadModel], 1, 10, 1));

        var result = await _handler.Handle(new GetOrdersByCustomerQuery { CustomerId = customerId }, CancellationToken.None);

        var order = Assert.Single(result.Items);
        Assert.Equal(OrderStatus.PaymentRejected, order.Status);
        Assert.Equal(OrderRejectionReason.InvalidCustomerAddress, order.RejectionReason);
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidCustomerIdException_WhenCustomerIdIsEmpty()
    {
        var act = () => _handler.Handle(new GetOrdersByCustomerQuery { CustomerId = Guid.Empty }, CancellationToken.None);

        await Assert.ThrowsAsync<InvalidCustomerIdException>(act);
    }

    [Fact]
    public async Task Handle_ShouldPreservePaginationMetadata_WhenRepositoryReturnsPagedResult()
    {
        var customerId = Guid.NewGuid();
        var paged = PagedResult<OrderReadModel>.Create([OrderTestData.CreateReadModel()], 2, 5, 7);

        _repositoryMock
            .Setup(repository => repository.GetByCustomerIdAsync(
                customerId,
                It.IsAny<GetOrdersByCustomerQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(paged);

        var result = await _handler.Handle(
            new GetOrdersByCustomerQuery { CustomerId = customerId, PageNumber = 2, PageSize = 5 },
            CancellationToken.None);

        Assert.Equal(2, result.Pagination.PageNumber);
        Assert.Equal(5, result.Pagination.PageSize);
        Assert.Equal(7, result.Pagination.TotalItems);
        Assert.Equal(2, result.Pagination.TotalPages);
    }
}
