using System.Security.Claims;
using ECommerce.Shared.Contracts;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using Payment.API.Controllers;
using Payment.Application.DTOs;
using Payment.Application.Interfaces;
using Payment.Application.Queries;
using Payment.Domain.Enums;

namespace Payment.Tests.Controllers;

public class PaymentsControllerTests
{
    [Fact]
    public void GetConfig_ShouldReturnPublishableKey()
    {
        var controller = CreateController(
            configuration: new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Stripe:PublishableKey"] = "pk_test_123"
                })
                .Build());

        var result = controller.GetConfig().Result as OkObjectResult;

        Assert.NotNull(result);
        var response = Assert.IsType<ApiResponse<object>>(result!.Value);
        Assert.NotNull(response.Data);
    }

    [Fact]
    public async Task GetByOrderId_ShouldReturnUnauthorized_WhenCustomerClaimIsMissing()
    {
        var controller = CreateController();
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity())
            }
        };

        var result = await controller.GetByOrderId(Guid.NewGuid());

        Assert.IsType<UnauthorizedObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetByOrderId_ShouldReturnAccepted_WhenPaymentIsStillBeingPrepared()
    {
        var mediator = new Mock<IMediator>();
        var accessClient = new Mock<IOrderPaymentAccessClient>();
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        mediator.Setup(item => item.Send(It.IsAny<GetPaymentByOrderIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PaymentDto?)null);
        accessClient.Setup(item => item.ValidateAsync(orderId, customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, true));

        var controller = CreateController(mediator.Object, accessClient.Object, customerId);

        var result = await controller.GetByOrderId(orderId);

        Assert.IsType<AcceptedResult>(result.Result);
    }

    [Fact]
    public async Task GetByOrderId_ShouldReturnNotFound_WhenOrderDoesNotBelongToCustomer()
    {
        var mediator = new Mock<IMediator>();
        var accessClient = new Mock<IOrderPaymentAccessClient>();
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        mediator.Setup(item => item.Send(It.IsAny<GetPaymentByOrderIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PaymentDto?)null);
        accessClient.Setup(item => item.ValidateAsync(orderId, customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, false));

        var controller = CreateController(mediator.Object, accessClient.Object, customerId);

        var result = await controller.GetByOrderId(orderId);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetByOrderId_ShouldReturnNotFound_WhenPaymentBelongsToAnotherCustomer()
    {
        var mediator = new Mock<IMediator>();
        mediator.Setup(item => item.Send(It.IsAny<GetPaymentByOrderIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentDto
            {
                OrderId = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                PaymentMethod = PaymentMethod.Card,
                Status = PaymentStatus.Pending
            });

        var controller = CreateController(mediator.Object, Mock.Of<IOrderPaymentAccessClient>(), Guid.NewGuid());

        var result = await controller.GetByOrderId(Guid.NewGuid());

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetByOrderId_ShouldReturnOk_WhenPaymentBelongsToCustomer()
    {
        var customerId = Guid.NewGuid();
        var payment = new PaymentDto
        {
            OrderId = Guid.NewGuid(),
            CustomerId = customerId,
            PaymentMethod = PaymentMethod.Card,
            Status = PaymentStatus.PendingConfirmation
        };

        var mediator = new Mock<IMediator>();
        mediator.Setup(item => item.Send(It.IsAny<GetPaymentByOrderIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        var controller = CreateController(mediator.Object, Mock.Of<IOrderPaymentAccessClient>(), customerId);

        var result = await controller.GetByOrderId(payment.OrderId);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<PaymentDto?>>(okResult.Value);
        Assert.Equal(payment.OrderId, response.Data!.OrderId);
    }

    private static PaymentsController CreateController(
        IMediator? mediator = null,
        IOrderPaymentAccessClient? accessClient = null,
        Guid? customerId = null,
        IConfiguration? configuration = null)
    {
        var controller = new PaymentsController(
            mediator ?? Mock.Of<IMediator>(),
            configuration ?? new ConfigurationBuilder().Build(),
            accessClient ?? Mock.Of<IOrderPaymentAccessClient>());

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = customerId.HasValue
                    ? new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim("customerId", customerId.Value.ToString())
                    }, "test"))
                    : new ClaimsPrincipal(new ClaimsIdentity())
            }
        };

        return controller;
    }
}
