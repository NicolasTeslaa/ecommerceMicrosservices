using Microsoft.AspNetCore.SignalR;
using Moq;
using Payment.API.Hubs;

namespace Payment.Tests.Hubs;

public class SignalRPaymentRealtimeNotifierTests
{
    [Fact]
    public async Task NotifyUpdatedAsync_ShouldSendMessageToOrderGroup()
    {
        var orderId = Guid.NewGuid();
        var clientProxy = new Mock<IClientProxy>();
        var clients = new Mock<IHubClients>();
        clients.Setup(item => item.Group(PaymentStatusHub.BuildOrderGroup(orderId)))
            .Returns(clientProxy.Object);

        var hubContext = new Mock<IHubContext<PaymentStatusHub>>();
        hubContext.SetupGet(item => item.Clients).Returns(clients.Object);

        var notifier = new SignalRPaymentRealtimeNotifier(hubContext.Object);

        await notifier.NotifyUpdatedAsync(orderId);

        clientProxy.Verify(item => item.SendCoreAsync("payment-updated", It.Is<object?[]>(args => (Guid)args[0]! == orderId), It.IsAny<CancellationToken>()), Times.Once);
    }
}
