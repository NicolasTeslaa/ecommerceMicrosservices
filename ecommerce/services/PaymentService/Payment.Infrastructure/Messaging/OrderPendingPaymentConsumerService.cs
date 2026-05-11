using System.Text.Json;
using Confluent.Kafka;
using ECommerce.Shared.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Payment.Application.Interfaces;
using Payment.Domain.Entities;
using Payment.Domain.Enums;
using Payment.Infrastructure.Persistence;

namespace Payment.Infrastructure.Messaging;

public class OrderPendingPaymentConsumerService : BackgroundService
{
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(10);

    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OrderPendingPaymentConsumerService> _logger;

    public OrderPendingPaymentConsumerService(
        IServiceScopeFactory serviceScopeFactory,
        IConfiguration configuration,
        ILogger<OrderPendingPaymentConsumerService> logger)
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
                    _logger.LogWarning("Payment pending-order consumer is waiting because Kafka:BootstrapServers was not configured.");
                    await Task.Delay(RetryDelay, stoppingToken);
                    continue;
                }

                var topic = _configuration["Kafka:OrderPendingPaymentTopic"] ?? "order.pending-payment";
                var groupId = _configuration["Kafka:OrderPendingPaymentConsumerGroup"] ?? "payment-service";

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
                    await ProcessMessageAsync(scope.ServiceProvider, result, groupId, stoppingToken);
                    consumer.Commit(result);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Unexpected error while consuming order.pending-payment.");
                await Task.Delay(RetryDelay, stoppingToken);
            }
        }
    }

    private async Task ProcessMessageAsync(
        IServiceProvider serviceProvider,
        ConsumeResult<string, string> result,
        string consumerGroup,
        CancellationToken cancellationToken)
    {
        var dbContext = serviceProvider.GetRequiredService<PaymentDbContext>();
        var repository = serviceProvider.GetRequiredService<IPaymentRepository>();
        var stripeGateway = serviceProvider.GetRequiredService<IStripePaymentGateway>();
        var eventPublisher = serviceProvider.GetRequiredService<IPaymentEventPublisher>();
        var realtimeNotifier = serviceProvider.GetRequiredService<IPaymentRealtimeNotifier>();

        var alreadyProcessed = await dbContext.ProcessedKafkaMessages.AnyAsync(
            message => message.Topic == result.Topic
                && message.Partition == result.Partition.Value
                && message.Offset == result.Offset.Value,
            cancellationToken);

        if (alreadyProcessed)
            return;

        var integrationEvent = JsonSerializer.Deserialize<OrderCreatedIntegrationEvent>(result.Message.Value);

        if (integrationEvent is null)
            return;

        var existingPayment = await repository.GetByOrderIdAsync(integrationEvent.OrderId, cancellationToken);

        if (existingPayment is null)
        {
            var paymentMethod = MapPaymentMethod(integrationEvent.PaymentMethod);
            var payment = new Payment.Domain.Entities.Payment(
                integrationEvent.OrderId,
                integrationEvent.CustomerId,
                integrationEvent.TotalAmount,
                _configuration["Stripe:Currency"] ?? "brl",
                paymentMethod);

            await repository.AddAsync(payment, cancellationToken);
            existingPayment = payment;
        }

        if (existingPayment.PaymentMethod != PaymentMethod.Card)
        {
            if (existingPayment.Status != PaymentStatus.Failed)
            {
                existingPayment.MarkFailed(
                    PaymentFailureReason.InvalidPaymentMethod,
                    $"Payment method '{integrationEvent.PaymentMethod}' is not supported by Stripe card integration.");
                await repository.UpdateAsync(existingPayment, cancellationToken);
                await eventPublisher.PublishFailedAsync(existingPayment, cancellationToken);
            }

            await RegisterMessageAsProcessedAsync(dbContext, result, consumerGroup, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            await realtimeNotifier.NotifyUpdatedAsync(existingPayment.OrderId, cancellationToken);
            return;
        }

        if (string.IsNullOrWhiteSpace(existingPayment.StripePaymentIntentId))
        {
            try
            {
                var intent = await stripeGateway.CreatePaymentIntentAsync(
                    integrationEvent.OrderId,
                    integrationEvent.CustomerId,
                    integrationEvent.TotalAmount,
                    _configuration["Stripe:Currency"] ?? "brl",
                    cancellationToken);

                existingPayment.SetPaymentIntent(intent.PaymentIntentId, intent.ClientSecret, intent.PaymentMethodId);
                await repository.UpdateAsync(existingPayment, cancellationToken);
            }
            catch (Exception exception)
            {
                _logger.LogWarning(
                    exception,
                    "Failed to create Stripe PaymentIntent for order '{OrderId}'. The Kafka message will be retried.",
                    integrationEvent.OrderId);
                return;
            }
        }

        await RegisterMessageAsProcessedAsync(dbContext, result, consumerGroup, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        await realtimeNotifier.NotifyUpdatedAsync(existingPayment.OrderId, cancellationToken);
    }

    private static async Task RegisterMessageAsProcessedAsync(
        PaymentDbContext dbContext,
        ConsumeResult<string, string> result,
        string consumerGroup,
        CancellationToken cancellationToken)
    {
        await dbContext.ProcessedKafkaMessages.AddAsync(
            new ProcessedKafkaMessage(
                result.Topic,
                result.Partition.Value,
                result.Offset.Value,
                consumerGroup,
                result.Message.Key ?? string.Empty,
                nameof(OrderCreatedIntegrationEvent)),
            cancellationToken);
    }

    private static PaymentMethod MapPaymentMethod(string paymentMethod)
    {
        return paymentMethod.Trim().ToLowerInvariant() switch
        {
            "credit" => PaymentMethod.Card,
            "debit" => PaymentMethod.Card,
            "pix" => PaymentMethod.Pix,
            _ => PaymentMethod.Unknown
        };
    }
}
