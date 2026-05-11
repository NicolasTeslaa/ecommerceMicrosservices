using System.Text.Json;
using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Order.Application.DTOs;
using Order.Application.Interfaces;
using Order.Domain.Entities;
using Order.Domain.Enums;
using Order.Domain.Exceptions;
using Order.Infrastructure.Persistence;

namespace Order.Infrastructure.Messaging;

public class OrderProcessorConsumerService : BackgroundService
{
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(10);

    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OrderProcessorConsumerService> _logger;

    public OrderProcessorConsumerService(
        IServiceScopeFactory serviceScopeFactory,
        IConfiguration configuration,
        ILogger<OrderProcessorConsumerService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var bootstrapServers = _configuration["Kafka:BootstrapServers"];

                if (string.IsNullOrWhiteSpace(bootstrapServers))
                {
                    _logger.LogWarning("Order processor consumer is waiting because Kafka:BootstrapServers was not configured.");
                    await Task.Delay(RetryDelay, stoppingToken);
                    continue;
                }

                var topic = _configuration["Kafka:OrderProcessingTopic"] ?? "order.processing.requested";
                var groupId = _configuration["Kafka:OrderProcessingConsumerGroup"] ?? "order-processor";

                using var consumer = new ConsumerBuilder<string, string>(new ConsumerConfig
                {
                    BootstrapServers = bootstrapServers,
                    GroupId = groupId,
                    AutoOffsetReset = AutoOffsetReset.Earliest,
                    AllowAutoCreateTopics = true,
                    EnableAutoCommit = false
                }).Build();

                consumer.Subscribe(topic);

                while (!stoppingToken.IsCancellationRequested)
                {
                    var result = consumer.Consume(stoppingToken);

                    if (string.IsNullOrWhiteSpace(result?.Message?.Value))
                        continue;

                    using var scope = _serviceScopeFactory.CreateScope();
                    await ProcessMessageAsync(scope.ServiceProvider, result.Message.Value, stoppingToken);
                    consumer.Commit(result);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Unexpected error while consuming order.processing.requested.");
                await Task.Delay(RetryDelay, stoppingToken);
            }
        }
    }

    private async Task ProcessMessageAsync(IServiceProvider serviceProvider, string queuePayload, CancellationToken cancellationToken)
    {
        using var document = JsonDocument.Parse(queuePayload);

        if (!document.RootElement.TryGetProperty("OutboxMessageId", out var outboxMessageIdElement))
            return;

        if (!Guid.TryParse(outboxMessageIdElement.GetString(), out var outboxMessageId))
            return;

        var writeDbContext = serviceProvider.GetRequiredService<OrderWriteDbContext>();
        var readModelProjector = serviceProvider.GetRequiredService<IOrderReadModelProjector>();
        var customerAddressValidationClient = serviceProvider.GetRequiredService<ICustomerAddressValidationClient>();
        var inventoryOrderReservationClient = serviceProvider.GetRequiredService<IInventoryOrderReservationClient>();
        var eventPublisher = serviceProvider.GetRequiredService<IOrderEventPublisher>();

        var outboxMessage = await writeDbContext.OrderProcessingOutboxMessages
            .FirstOrDefaultAsync(message => message.Id == outboxMessageId, cancellationToken);

        if (outboxMessage is null || outboxMessage.ProcessedAtUtc is not null)
            return;

        var request = JsonSerializer.Deserialize<OrderProcessingRequestDto>(outboxMessage.Payload);
        if (request is null)
        {
            _logger.LogWarning("Order processor ignored outbox message '{OutboxMessageId}' because the payload was invalid.", outboxMessageId);
            outboxMessage.RegisterProcessingFailure("Invalid order processing payload.");
            await writeDbContext.SaveChangesAsync(cancellationToken);
            return;
        }

        var availabilityResult = await inventoryOrderReservationClient.ReserveAsync(
            request.OrderId,
            request.CustomerId,
            request.Items
                .Select(item => new ProductAvailabilityCheckItemDto
                {
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    RequestedQuantity = item.Quantity
                })
                .ToArray(),
            cancellationToken);

        if (!availabilityResult.IsValid)
        {
            var rejectedOrder = CreateRejectedOrderFromAvailability(request, availabilityResult);

            await PersistRejectedOrderAsync(writeDbContext, readModelProjector, rejectedOrder, cancellationToken);

            _logger.LogWarning(
                "Order '{OrderId}' was rejected before persistence. Reason: {Reason}",
                request.OrderId,
                availabilityResult.Reason);

            await eventPublisher.PublishOrderRejectedAsync(
                request.OrderId,
                request.CustomerId,
                request.CustomerAddressId,
                request.RequestedAtUtc,
                availabilityResult.Reason,
                availabilityResult.Issues,
                cancellationToken);

            outboxMessage.MarkAsProcessed();
            await writeDbContext.SaveChangesAsync(cancellationToken);
            return;
        }

        var existingOrder = await writeDbContext.Orders
            .Include(order => order.Items)
            .FirstOrDefaultAsync(order => order.Id == request.OrderId, cancellationToken);

        Order.Domain.Entities.Order order;

        if (existingOrder is null)
        {
            ValidatedCustomerAddressDto validatedAddress;

            try
            {
                validatedAddress = await customerAddressValidationClient.ValidateAsync(
                    request.CustomerId,
                    request.CustomerAddressId,
                    cancellationToken);
            }
            catch (CustomerAddressNotFoundException exception)
            {
                await inventoryOrderReservationClient.ReleaseAsync(request.OrderId, cancellationToken);
                var rejectedOrder = CreateRejectedOrderForAddress(request, exception.Message);
                await PersistRejectedOrderAsync(writeDbContext, readModelProjector, rejectedOrder, cancellationToken);

                await eventPublisher.PublishOrderRejectedAsync(
                    request.OrderId,
                    request.CustomerId,
                    request.CustomerAddressId,
                    request.RequestedAtUtc,
                    exception.Message,
                    Array.Empty<ProductAvailabilityIssueDto>(),
                    cancellationToken);

                outboxMessage.MarkAsProcessed();
                await writeDbContext.SaveChangesAsync(cancellationToken);
                return;
            }

            var items = request.Items
                .Select(item => new OrderItem(item.ProductId, item.ProductName, item.UnitPrice, item.Quantity))
                .ToArray();

            order = new Order.Domain.Entities.Order(
                request.OrderId,
                request.CustomerId,
                request.CustomerAddressId,
                validatedAddress.CustomerEmail,
                validatedAddress.FormattedAddress,
                request.ShippingAmount,
                request.PaymentMethod,
                request.PaymentToken,
                request.PaymentCardBrand,
                request.PaymentCardLast4,
                items,
                request.RequestedAtUtc);

            await writeDbContext.Orders.AddAsync(order, cancellationToken);
            await writeDbContext.SaveChangesAsync(cancellationToken);
        }
        else
        {
            order = existingOrder;
        }

        try
        {
            await readModelProjector.ProjectAsync(order, cancellationToken);
            await eventPublisher.PublishOrderCreatedAsync(order, cancellationToken);
            outboxMessage.MarkAsProcessed();
            await writeDbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            outboxMessage.RegisterProcessingFailure(exception.Message);
            await writeDbContext.SaveChangesAsync(cancellationToken);
            _logger.LogError(exception, "Order processor failed to project or publish order '{OrderId}'.", order.Id);
        }
    }

    private static Order.Domain.Entities.Order CreateRejectedOrderFromAvailability(
        OrderProcessingRequestDto request,
        ProductAvailabilityValidationResultDto availabilityResult)
    {
        var items = request.Items
            .Select(item => new OrderItem(item.ProductId, item.ProductName, item.UnitPrice, item.Quantity))
            .ToArray();

        var rejectionReason = availabilityResult.Issues.Any(issue => issue.Reason.Contains("stock", StringComparison.OrdinalIgnoreCase))
            ? OrderRejectionReason.InsufficientStock
            : OrderRejectionReason.ProductUnavailable;

        return Order.Domain.Entities.Order.CreateRejected(
            request.OrderId,
            request.CustomerId,
            request.CustomerAddressId,
            request.ShippingAmount,
            request.PaymentMethod,
            request.PaymentToken,
            request.PaymentCardBrand,
            request.PaymentCardLast4,
            items,
            request.RequestedAtUtc,
            rejectionReason,
            availabilityResult.Reason);
    }

    private static Order.Domain.Entities.Order CreateRejectedOrderForAddress(OrderProcessingRequestDto request, string reason)
    {
        var items = request.Items
            .Select(item => new OrderItem(item.ProductId, item.ProductName, item.UnitPrice, item.Quantity))
            .ToArray();

        return Order.Domain.Entities.Order.CreateRejected(
            request.OrderId,
            request.CustomerId,
            request.CustomerAddressId,
            request.ShippingAmount,
            request.PaymentMethod,
            request.PaymentToken,
            request.PaymentCardBrand,
            request.PaymentCardLast4,
            items,
            request.RequestedAtUtc,
            OrderRejectionReason.InvalidCustomerAddress,
            reason);
    }

    private static async Task PersistRejectedOrderAsync(
        OrderWriteDbContext writeDbContext,
        IOrderReadModelProjector readModelProjector,
        Order.Domain.Entities.Order rejectedOrder,
        CancellationToken cancellationToken)
    {
        var existingOrder = await writeDbContext.Orders
            .Include(order => order.Items)
            .FirstOrDefaultAsync(order => order.Id == rejectedOrder.Id, cancellationToken);

        if (existingOrder is null)
        {
            await writeDbContext.Orders.AddAsync(rejectedOrder, cancellationToken);
            await writeDbContext.SaveChangesAsync(cancellationToken);
        }

        await readModelProjector.ProjectAsync(rejectedOrder, cancellationToken);
    }
}
