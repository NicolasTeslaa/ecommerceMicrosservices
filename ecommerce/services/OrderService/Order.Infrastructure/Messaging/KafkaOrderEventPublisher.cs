using System.Text.Json;
using Confluent.Kafka;
using ECommerce.Shared.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Order.Application.DTOs;
using Order.Application.Interfaces;

namespace Order.Infrastructure.Messaging;

public class KafkaOrderEventPublisher : IOrderEventPublisher
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<KafkaOrderEventPublisher> _logger;

    public KafkaOrderEventPublisher(IConfiguration configuration, ILogger<KafkaOrderEventPublisher> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task PublishOrderCreatedAsync(Order.Domain.Entities.Order order, CancellationToken cancellationToken = default)
    {
        var bootstrapServers = _configuration["Kafka:BootstrapServers"];

        if (string.IsNullOrWhiteSpace(bootstrapServers))
        {
            _logger.LogWarning("Order created event was not published because Kafka:BootstrapServers was not configured.");
            return;
        }

        var topic = _configuration["Kafka:OrderPendingPaymentTopic"] ?? "order.pending-payment";
        var producerConfig = new ProducerConfig { BootstrapServers = bootstrapServers };

        var integrationEvent = new OrderCreatedIntegrationEvent
        {
            OrderId = order.Id,
            CustomerId = order.CustomerId,
            CustomerAddressId = order.CustomerAddressId,
            CustomerEmail = order.CustomerEmail,
            ShippingAmount = order.ShippingAmount,
            PaymentMethod = order.PaymentMethod.ToString(),
            PaymentCardBrand = order.PaymentCardBrand,
            PaymentCardLast4 = order.PaymentCardLast4,
            PaymentToken = order.PaymentToken,
            TotalAmount = order.TotalAmount,
            Status = order.Status.ToString(),
            CreatedAtUtc = order.CreatedAtUtc,
            Items = order.Items
                .Select(item => new OrderCreatedIntegrationEventItem
                {
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    UnitPrice = item.UnitPrice,
                    Quantity = item.Quantity,
                    TotalPrice = item.TotalPrice
                })
                .ToArray()
        };

        try
        {
            using var producer = new ProducerBuilder<string, string>(producerConfig).Build();
            await producer.ProduceAsync(
                topic,
                new Message<string, string>
                {
                    Key = order.Id.ToString(),
                    Value = JsonSerializer.Serialize(integrationEvent)
                },
                cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to publish order.pending-payment event for order '{OrderId}'.", order.Id);
        }
    }

    public async Task PublishOrderConfirmedAsync(Order.Domain.Entities.Order order, CancellationToken cancellationToken = default)
    {
        var bootstrapServers = _configuration["Kafka:BootstrapServers"];

        if (string.IsNullOrWhiteSpace(bootstrapServers))
        {
            _logger.LogWarning("Order confirmed event was not published because Kafka:BootstrapServers was not configured.");
            return;
        }

        var topic = _configuration["Kafka:OrderConfirmedTopic"] ?? "order.confirmed";
        var producerConfig = new ProducerConfig { BootstrapServers = bootstrapServers };

        var integrationEvent = new OrderConfirmedIntegrationEvent
        {
            OrderId = order.Id,
            CustomerId = order.CustomerId,
            CustomerAddressId = order.CustomerAddressId,
            CustomerEmail = order.CustomerEmail,
            ShippingAmount = order.ShippingAmount,
            TotalAmount = order.TotalAmount,
            Currency = "brl",
            Status = order.Status.ToString(),
            ConfirmedAtUtc = DateTime.UtcNow,
            Items = order.Items
                .Select(item => new OrderConfirmedIntegrationEventItem
                {
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    UnitPrice = item.UnitPrice,
                    Quantity = item.Quantity,
                    TotalPrice = item.TotalPrice
                })
                .ToArray()
        };

        try
        {
            using var producer = new ProducerBuilder<string, string>(producerConfig).Build();
            await producer.ProduceAsync(
                topic,
                new Message<string, string>
                {
                    Key = order.Id.ToString(),
                    Value = JsonSerializer.Serialize(integrationEvent)
                },
                cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to publish order.confirmed event for order '{OrderId}'.", order.Id);
        }
    }

    public async Task PublishOrderRejectedAsync(
        Guid orderId,
        Guid customerId,
        Guid customerAddressId,
        DateTime requestedAtUtc,
        string reason,
        IReadOnlyCollection<ProductAvailabilityIssueDto> issues,
        CancellationToken cancellationToken = default)
    {
        var bootstrapServers = _configuration["Kafka:BootstrapServers"];

        if (string.IsNullOrWhiteSpace(bootstrapServers))
        {
            _logger.LogWarning("Order rejected event was not published because Kafka:BootstrapServers was not configured.");
            return;
        }

        var topic = _configuration["Kafka:OrderRejectedTopic"] ?? "order.rejected";
        var producerConfig = new ProducerConfig { BootstrapServers = bootstrapServers };

        var integrationEvent = new OrderRejectedIntegrationEvent
        {
            OrderId = orderId,
            CustomerId = customerId,
            CustomerAddressId = customerAddressId,
            RequestedAtUtc = requestedAtUtc,
            RejectedAtUtc = DateTime.UtcNow,
            Reason = reason,
            Items = issues
                .Select(item => new OrderRejectedIntegrationEventItem
                {
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    RequestedQuantity = item.RequestedQuantity,
                    AvailableQuantity = item.AvailableQuantity,
                    Reason = item.Reason
                })
                .ToArray()
        };

        try
        {
            using var producer = new ProducerBuilder<string, string>(producerConfig).Build();
            await producer.ProduceAsync(
                topic,
                new Message<string, string>
                {
                    Key = orderId.ToString(),
                    Value = JsonSerializer.Serialize(integrationEvent)
                },
                cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to publish order.rejected event for order '{OrderId}'.", orderId);
        }
    }
}
