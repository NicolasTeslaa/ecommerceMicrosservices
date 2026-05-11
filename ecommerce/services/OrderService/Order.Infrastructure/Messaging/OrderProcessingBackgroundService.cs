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

namespace Order.Infrastructure.Messaging;

public class OrderProcessingBackgroundService : BackgroundService
{
    private static readonly TimeSpan IdleDelay = TimeSpan.FromSeconds(5);
    private const int BatchSize = 20;

    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OrderProcessingBackgroundService> _logger;

    public OrderProcessingBackgroundService(
        IServiceScopeFactory serviceScopeFactory,
        IConfiguration configuration,
        ILogger<OrderProcessingBackgroundService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();

        using var consumer = BuildConsumer();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var writeDbContext = scope.ServiceProvider.GetRequiredService<OrderWriteDbContext>();

                if (consumer is not null)
                {
                    var result = consumer.Consume(TimeSpan.FromMilliseconds(250));

                    if (result?.Message?.Value is not null)
                        await ProcessQueueSignalAsync(scope.ServiceProvider, writeDbContext, result.Message.Value, stoppingToken);
                }

                var pendingMessages = await writeDbContext.OrderProcessingOutboxMessages
                    .Where(message => message.ProcessedAtUtc == null)
                    .OrderBy(message => message.RequestedAtUtc)
                    .Take(BatchSize)
                    .ToListAsync(stoppingToken);

                if (pendingMessages.Count == 0)
                {
                    await Task.Delay(IdleDelay, stoppingToken);
                    continue;
                }

                foreach (var message in pendingMessages)
                    await ProcessMessageAsync(scope.ServiceProvider, writeDbContext, message, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Unexpected error while processing order outbox messages.");
                await Task.Delay(IdleDelay, stoppingToken);
            }
        }
    }

    private IConsumer<string, string>? BuildConsumer()
    {
        var bootstrapServers = _configuration["Kafka:BootstrapServers"];

        if (string.IsNullOrWhiteSpace(bootstrapServers))
            return null;

        var topic = _configuration["Kafka:OrderProcessingTopic"] ?? "order.processing.requested";
        var groupId = _configuration["Kafka:OrderProcessingConsumerGroup"] ?? "order-service-processing";

        var consumer = new ConsumerBuilder<string, string>(new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
            GroupId = groupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            AllowAutoCreateTopics = true,
            EnableAutoCommit = true
        }).Build();

        consumer.Subscribe(topic);
        return consumer;
    }

    private async Task ProcessQueueSignalAsync(
        IServiceProvider serviceProvider,
        OrderWriteDbContext writeDbContext,
        string queuePayload,
        CancellationToken cancellationToken)
    {
        try
        {
            using var document = JsonDocument.Parse(queuePayload);

            if (!document.RootElement.TryGetProperty("OutboxMessageId", out var outboxMessageIdElement))
                return;

            if (!Guid.TryParse(outboxMessageIdElement.GetString(), out var outboxMessageId))
                return;

            var outboxMessage = await writeDbContext.OrderProcessingOutboxMessages
                .FirstOrDefaultAsync(message => message.Id == outboxMessageId, cancellationToken);

            if (outboxMessage is null || outboxMessage.ProcessedAtUtc is not null)
                return;

            await ProcessMessageAsync(serviceProvider, writeDbContext, outboxMessage, cancellationToken);
        }
        catch (JsonException exception)
        {
            _logger.LogWarning(exception, "Invalid internal order processing queue payload received.");
        }
    }

    private async Task ProcessMessageAsync(
        IServiceProvider serviceProvider,
        OrderWriteDbContext writeDbContext,
        OrderProcessingOutboxMessage message,
        CancellationToken cancellationToken)
    {
        try
        {
            var request = JsonSerializer.Deserialize<OrderProcessingRequestDto>(message.Payload);
            if (request is null)
            {
                message.RegisterProcessingFailure("Invalid order processing payload.");
                await writeDbContext.SaveChangesAsync(cancellationToken);
                _logger.LogError("Outbox message '{OutboxMessageId}' has an invalid payload.", message.Id);
                return;
            }

            var customerAddressValidationClient = serviceProvider.GetRequiredService<ICustomerAddressValidationClient>();
            var readModelProjector = serviceProvider.GetRequiredService<IOrderReadModelProjector>();
            var eventPublisher = serviceProvider.GetRequiredService<IOrderEventPublisher>();

            var existingOrder = await writeDbContext.Orders
                .Include(order => order.Items)
                .FirstOrDefaultAsync(order => order.Id == request.OrderId, cancellationToken);

            Order.Domain.Entities.Order order;

            if (existingOrder is null)
            {
                var validatedAddress = await customerAddressValidationClient.ValidateAsync(
                    request.CustomerId,
                    request.CustomerAddressId,
                    cancellationToken);

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

            await readModelProjector.ProjectAsync(order, cancellationToken);
            await eventPublisher.PublishOrderCreatedAsync(order, cancellationToken);

            message.MarkProcessed();
            await writeDbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            message.RegisterProcessingFailure(exception.Message);
            await writeDbContext.SaveChangesAsync(cancellationToken);

            _logger.LogError(
                exception,
                "Failed to process order outbox message '{OutboxMessageId}' for order '{OrderId}'. Attempt {Attempt}.",
                message.Id,
                message.OrderId,
                message.ProcessingAttempts);
        }
    }
}
