using ECommerce.Shared.Contracts;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Order.API.Read.Controllers;
using Order.Application.DTOs;
using Order.Application.Queries;
using Order.Domain.Enums;
using Order.Tests.Support;

namespace Order.Tests.Controllers;

public class ReadOrdersControllerTests
{
    private readonly Mock<IMediator> _mediatorMock = new();
    private readonly OrdersController _controller;

    public ReadOrdersControllerTests()
    {
        _controller = new OrdersController(_mediatorMock.Object);
    }

    [Fact]
    public async Task GetById_ShouldReturnOkResponse_WhenMediatorReturnsOrder()
    {
        var order = new OrderDto
        {
            Id = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            CustomerAddressId = Guid.NewGuid(),
            CustomerEmail = "customer@example.com",
            ShippingAddress = "Rua A, 123",
            PaymentMethod = PaymentMethod.Pix,
            Status = OrderStatus.PendingPayment
        };

        _mediatorMock
            .Setup(mediator => mediator.Send(It.IsAny<GetOrderByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var result = await _controller.GetById(order.Id);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<OrderDto>>(ok.Value);
        Assert.True(response.Success);
        Assert.Equal(order.Id, response.Data!.Id);
        Assert.Equal("Order retrieved successfully.", response.Message);
    }

    [Fact]
    public async Task GetById_ShouldSendOrderIdQueryToMediator()
    {
        var orderId = Guid.NewGuid();

        _mediatorMock
            .Setup(mediator => mediator.Send(It.IsAny<GetOrderByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrderDto { Id = orderId });

        await _controller.GetById(orderId);

        _mediatorMock.Verify(
            mediator => mediator.Send(
                It.Is<GetOrderByIdQuery>(query => query.OrderId == orderId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByCustomer_ShouldReturnOkResponse_WhenMediatorReturnsPagedOrders()
    {
        var customerId = Guid.NewGuid();
        var orders = new[]
        {
            new OrderDto { Id = Guid.NewGuid(), CustomerId = customerId, PaymentMethod = PaymentMethod.Credit, Status = OrderStatus.PendingPayment },
            new OrderDto { Id = Guid.NewGuid(), CustomerId = customerId, PaymentMethod = PaymentMethod.Pix, Status = OrderStatus.PaymentRejected }
        };

        _mediatorMock
            .Setup(mediator => mediator.Send(It.IsAny<GetOrdersByCustomerQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(PagedResult<OrderDto>.Create(orders, 1, 10, 2));

        var result = await _controller.GetByCustomer(customerId, new GetOrdersByCustomerQuery { PageNumber = 1, PageSize = 10 });

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<IReadOnlyCollection<OrderDto>>>(ok.Value);
        Assert.True(response.Success);
        Assert.Equal(2, response.Data!.Count);
        Assert.Equal(2, response.Pagination!.TotalItems);
    }

    [Fact]
    public async Task GetByCustomer_ShouldForwardCustomerIdAndPagination()
    {
        var customerId = Guid.NewGuid();

        _mediatorMock
            .Setup(mediator => mediator.Send(It.IsAny<GetOrdersByCustomerQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(PagedResult<OrderDto>.Create([], 2, 5, 0));

        await _controller.GetByCustomer(customerId, new GetOrdersByCustomerQuery { PageNumber = 2, PageSize = 5 });

        _mediatorMock.Verify(
            mediator => mediator.Send(
                It.Is<GetOrdersByCustomerQuery>(query =>
                    query.CustomerId == customerId &&
                    query.PageNumber == 2 &&
                    query.PageSize == 5),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByCustomer_ShouldReturnRejectedOrderMetadata_WhenPresent()
    {
        var customerId = Guid.NewGuid();
        var rejectedOrder = new OrderDto
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            PaymentMethod = PaymentMethod.Pix,
            Status = OrderStatus.PaymentRejected,
            RejectionReason = OrderRejectionReason.ProductUnavailable,
            RejectionDetail = "Produto indisponivel."
        };

        _mediatorMock
            .Setup(mediator => mediator.Send(It.IsAny<GetOrdersByCustomerQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(PagedResult<OrderDto>.Create([rejectedOrder], 1, 10, 1));

        var result = await _controller.GetByCustomer(customerId, new GetOrdersByCustomerQuery());

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<IReadOnlyCollection<OrderDto>>>(ok.Value);
        var order = Assert.Single(response.Data!);
        Assert.Equal(OrderRejectionReason.ProductUnavailable, order.RejectionReason);
        Assert.Equal("Produto indisponivel.", order.RejectionDetail);
    }
}
