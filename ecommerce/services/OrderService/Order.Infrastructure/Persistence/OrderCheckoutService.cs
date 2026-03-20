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
    private readonly IConfiguration _configuration;

    public OrderCheckoutService(
        OrderWriteDbContext writeDbContext,
        IConfiguration configuration)
    {
        _writeDbContext = writeDbContext;
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
            PaymentToken = request.PaymentToken,
            PaymentCardBrand = request.PaymentCardBrand,
            PaymentCardLast4 = request.PaymentCardLast4,
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

        return new OrderProcessingAcceptedDto
        {
            OrderId = orderId,
            Status = "pending_payment",
            Message = "Pedido recebido. O pagamento sera validado antes da confirmacao final.",
            RequestedAtUtc = requestedAtUtc
        };
    }
}
