using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Order.Application.Commands;
using Order.Application.DTOs;
using Order.Application.Interfaces;
using Order.Domain.Entities;

namespace Order.Infrastructure.Persistence;

public class OrderCheckoutService : IOrderCheckoutService
{
    private readonly OrderWriteDbContext _writeDbContext;
    private readonly IOrderProcessingQueuePublisher _queuePublisher;
    private readonly IConfiguration _configuration;

    public OrderCheckoutService(
        OrderWriteDbContext writeDbContext,
        IOrderProcessingQueuePublisher queuePublisher,
        IConfiguration configuration)
    {
        _writeDbContext = writeDbContext;
        _queuePublisher = queuePublisher;
        _configuration = configuration;
    }

    public async Task<OrderProcessingAcceptedDto> QueueOrderAsync(CreateOrderCommand request, CancellationToken cancellationToken = default)
    {
        var requestedAtUtc = DateTime.UtcNow;
        var orderId = Guid.NewGuid();
        var processingRequest = new OrderProcessingRequestDto
        {
            OrderId = orderId,
            CustomerId = request.CustomerId,
            CustomerAddressId = request.CustomerAddressId,
            ShippingAmount = request.ShippingAmount,
            PaymentMethod = request.PaymentMethod,
            RequestedAtUtc = requestedAtUtc,
            Items = request.Items
                .Select(item => new OrderProcessingItemDto
                {
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    UnitPrice = item.UnitPrice,
                    Quantity = item.Quantity
                })
                .ToArray()
        };

        var internalTopic = _configuration["Kafka:OrderProcessingTopic"] ?? "order.processing.requested";
        var outboxMessage = OrderProcessingOutboxMessage.Create(
            orderId,
            internalTopic,
            nameof(OrderProcessingRequestDto),
            JsonSerializer.Serialize(processingRequest),
            requestedAtUtc);

        await _writeDbContext.OrderProcessingOutboxMessages.AddAsync(outboxMessage, cancellationToken);
        await _writeDbContext.SaveChangesAsync(cancellationToken);

        outboxMessage.MarkDispatchAttempt();
        var published = await _queuePublisher.TryPublishAsync(outboxMessage.Id, cancellationToken);

        if (published)
            outboxMessage.MarkAsDispatched();
        else
            outboxMessage.RegisterDispatchFailure("Initial dispatch to order.processing.requested failed.");

        await _writeDbContext.SaveChangesAsync(cancellationToken);

        return new OrderProcessingAcceptedDto
        {
            OrderId = orderId,
            Status = "pending",
            Message = "Pedido recebido. Ele sera processado em instantes e voce sera notificado apos a conclusao.",
            RequestedAtUtc = requestedAtUtc
        };
    }
}
