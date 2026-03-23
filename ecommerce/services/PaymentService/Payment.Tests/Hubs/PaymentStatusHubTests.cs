using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Moq;
using Payment.API.Hubs;
using Payment.Application.DTOs;
using Payment.Application.Interfaces;
using Payment.Application.Queries;
using Payment.Domain.Enums;
using Payment.Tests.Support;

namespace Payment.Tests.Hubs;

public class PaymentStatusHubTests
{
    [Fact]
    public async Task JoinOrderPayment_ShouldAddConnectionToGroup_WhenPaymentBelongsToCustomer()
    {
        var customerId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var mediator = new Mock<IMediator>();
        mediator.Setup(item => item.Send(It.IsAny<GetPaymentByOrderIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentDto
            {
                OrderId = orderId,
                CustomerId = customerId,
                PaymentMethod = PaymentMethod.Card,
                Status = PaymentStatus.Pending
            });

        var groups = new TestGroupManager();
        var hub = CreateHub(mediator.Object, Mock.Of<IOrderPaymentAccessClient>(), customerId, groups);

        await hub.JoinOrderPayment(orderId);

        Assert.Contains(groups.AddedGroups, group => group.GroupName == PaymentStatusHub.BuildOrderGroup(orderId));
    }

    [Fact]
    public async Task JoinOrderPayment_ShouldValidateOrderAccess_WhenPaymentDoesNotExistYet()
    {
        var customerId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var mediator = new Mock<IMediator>();
        mediator.Setup(item => item.Send(It.IsAny<GetPaymentByOrderIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PaymentDto?)null);

        var accessClient = new Mock<IOrderPaymentAccessClient>();
        accessClient.Setup(item => item.ValidateAsync(orderId, customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, true));

        var groups = new TestGroupManager();
        var hub = CreateHub(mediator.Object, accessClient.Object, customerId, groups);

        await hub.JoinOrderPayment(orderId);

        Assert.Single(groups.AddedGroups);
    }

    [Fact]
    public async Task JoinOrderPayment_ShouldThrow_WhenCustomerClaimIsMissing()
    {
        var hub = new PaymentStatusHub(Mock.Of<IMediator>(), Mock.Of<IOrderPaymentAccessClient>())
        {
            Context = new TestHubCallerContext(new ClaimsPrincipal(new ClaimsIdentity())),
            Groups = new TestGroupManager()
        };

        await Assert.ThrowsAsync<HubException>(() => hub.JoinOrderPayment(Guid.NewGuid()));
    }

    [Fact]
    public async Task JoinOrderPayment_ShouldThrow_WhenPaymentBelongsToAnotherCustomer()
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

        var hub = CreateHub(mediator.Object, Mock.Of<IOrderPaymentAccessClient>(), Guid.NewGuid(), new TestGroupManager());

        await Assert.ThrowsAsync<HubException>(() => hub.JoinOrderPayment(Guid.NewGuid()));
    }

    private static PaymentStatusHub CreateHub(IMediator mediator, IOrderPaymentAccessClient accessClient, Guid customerId, TestGroupManager groups)
    {
        return new PaymentStatusHub(mediator, accessClient)
        {
            Context = new TestHubCallerContext(new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim("customerId", customerId.ToString())
            }, "test"))),
            Groups = groups
        };
    }
}
