using Notification.Application.Handlers;
using Notification.Application.Interfaces;
using Notification.Application.Queries;
using Notification.Domain.Entities;

namespace Notification.Tests.Application;

public class GetNotificationsByOrderIdHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnGroupedNotifications()
    {
        var orderId = Guid.NewGuid();
        var repository = new FakeNotificationRepository(orderId);
        var handler = new GetNotificationsByOrderIdHandler(repository);

        var result = await handler.Handle(new GetNotificationsByOrderIdQuery(orderId), CancellationToken.None);

        Assert.Equal(orderId, result.OrderId);
        Assert.Single(result.Emails);
        Assert.Single(result.WhatsAppMessages);
    }

    private sealed class FakeNotificationRepository : INotificationRepository
    {
        private readonly Guid _orderId;

        public FakeNotificationRepository(Guid orderId)
        {
            _orderId = orderId;
        }

        public Task<IReadOnlyCollection<EmailNotification>> GetEmailNotificationsByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
        {
            IReadOnlyCollection<EmailNotification> notifications =
            [
                new EmailNotification(orderId, Guid.NewGuid(), "order.confirmed", "OrderConfirmedIntegrationEvent", "customer@example.com", "Pedido confirmado", "Seu pedido foi confirmado.", $"email:{orderId}")
            ];

            return Task.FromResult(notifications);
        }

        public Task<IReadOnlyCollection<WhatsAppNotification>> GetWhatsAppNotificationsByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
        {
            IReadOnlyCollection<WhatsAppNotification> notifications =
            [
                new WhatsAppNotification(orderId, Guid.NewGuid(), "order.confirmed", "OrderConfirmedIntegrationEvent", "11999999999", "Seu pedido foi confirmado.", $"whatsapp:{orderId}")
            ];

            return Task.FromResult(notifications);
        }
    }
}
