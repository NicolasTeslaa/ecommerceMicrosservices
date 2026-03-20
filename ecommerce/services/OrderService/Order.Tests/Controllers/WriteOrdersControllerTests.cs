using ECommerce.Shared.Contracts;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Order.API.Write.Controllers;
using Order.Application.Commands;
using Order.Application.DTOs;
using Order.Domain.Enums;
using Order.Tests.Support;

namespace Order.Tests.Controllers;

public class WriteOrdersControllerTests
{
    private readonly Mock<IMediator> _mediatorMock = new();
    private readonly OrdersController _controller;

    public WriteOrdersControllerTests()
    {
        _controller = new OrdersController(_mediatorMock.Object);
    }

    [Fact]
    public async Task Create_ShouldReturnAcceptedResponse_WhenMediatorAcceptsOrder()
    {
        var command = OrderTestData.CreateCommand(PaymentMethod.Credit);
        var accepted = new OrderProcessingAcceptedDto
        {
            OrderId = Guid.NewGuid(),
            Status = "pending_payment",
            Message = "Pedido recebido",
            RequestedAtUtc = DateTime.UtcNow
        };

        _mediatorMock
            .Setup(mediator => mediator.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(accepted);

        var result = await _controller.Create(command);

        var response = Assert.IsType<AcceptedResult>(result.Result);
        var payload = Assert.IsType<ApiResponse<OrderProcessingAcceptedDto>>(response.Value);
        Assert.True(payload.Success);
        Assert.Equal(accepted.OrderId, payload.Data!.OrderId);
    }

    [Fact]
    public async Task Create_ShouldForwardCommandToMediator()
    {
        var command = OrderTestData.CreateCommand(PaymentMethod.Pix);

        _mediatorMock
            .Setup(mediator => mediator.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrderProcessingAcceptedDto
            {
                OrderId = Guid.NewGuid(),
                Status = "pending_payment",
                Message = "ok",
                RequestedAtUtc = DateTime.UtcNow
            });

        await _controller.Create(command);

        _mediatorMock.Verify(
            mediator => mediator.Send(command, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Create_ShouldPreservePendingPaymentStatus_WhenMediatorReturnsAcceptedDto()
    {
        var command = OrderTestData.CreateCommand(PaymentMethod.Debit);
        var accepted = new OrderProcessingAcceptedDto
        {
            OrderId = Guid.NewGuid(),
            Status = "pending_payment",
            Message = "Pagamento pendente",
            RequestedAtUtc = DateTime.UtcNow
        };

        _mediatorMock
            .Setup(mediator => mediator.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(accepted);

        var result = await _controller.Create(command);

        var response = Assert.IsType<AcceptedResult>(result.Result);
        var payload = Assert.IsType<ApiResponse<OrderProcessingAcceptedDto>>(response.Value);
        Assert.Equal("pending_payment", payload.Data!.Status);
        Assert.Contains("Pedido recebido", payload.Message);
    }
}
