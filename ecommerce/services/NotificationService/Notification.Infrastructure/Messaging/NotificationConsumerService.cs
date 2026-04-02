using System.Text.Json;
using Confluent.Kafka;
using ECommerce.Shared.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Notification.Application.Interfaces;
using Notification.Domain.Entities;
using Notification.Infrastructure.Persistence;

namespace Notification.Infrastructure.Messaging;

public class NotificationConsumerService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<NotificationConsumerService> _logger;
    private readonly JsonSerializerOptions _serializerOptions = new() { PropertyNameCaseInsensitive = true };

    public NotificationConsumerService(
        IServiceScopeFactory serviceScopeFactory,
        IConfiguration configuration,
        ILogger<NotificationConsumerService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();
        await ConsumeAsync(stoppingToken);
    }

    private async Task ConsumeAsync(CancellationToken stoppingToken)
    {
        var bootstrapServers = _configuration["Kafka:BootstrapServers"];
        if (string.IsNullOrWhiteSpace(bootstrapServers))
            throw new InvalidOperationException("Kafka:BootstrapServers was not configured for NotificationService.");

        var groupId = _configuration["Kafka:ConsumerGroup"] ?? "notification-service";
        var topics = ResolveTopics();

        using var consumer = new ConsumerBuilder<string, string>(new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
            GroupId = groupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            AllowAutoCreateTopics = true
        }).Build();

        consumer.Subscribe(topics);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = consumer.Consume(stoppingToken);
                if (string.IsNullOrWhiteSpace(result?.Message?.Value))
                    continue;

                using var scope = _serviceScopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
                var customerContactClient = scope.ServiceProvider.GetRequiredService<ICustomerContactClient>();

                var alreadyProcessed = await dbContext.ProcessedKafkaMessages.AnyAsync(
                    item => item.Topic == result.Topic
                        && item.Partition == result.Partition.Value
                        && item.Offset == result.Offset.Value,
                    stoppingToken);

                if (alreadyProcessed)
                {
                    consumer.Commit(result);
                    continue;
                }

                await QueueNotificationsAsync(dbContext, customerContactClient, result.Topic, result.Message.Value, stoppingToken);

                await dbContext.ProcessedKafkaMessages.AddAsync(
                    new ProcessedKafkaMessage(
                        result.Topic,
                        result.Partition.Value,
                        result.Offset.Value,
                        groupId,
                        result.Message.Key ?? string.Empty,
                        ResolveMessageType(result.Topic)),
                    stoppingToken);

                await dbContext.SaveChangesAsync(stoppingToken);
                consumer.Commit(result);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Unexpected error while consuming notification topics.");
            }
        }
    }

    private async Task QueueNotificationsAsync(
        NotificationDbContext dbContext,
        ICustomerContactClient customerContactClient,
        string topic,
        string payload,
        CancellationToken cancellationToken)
    {
        if (topic == (_configuration["Kafka:OrderConfirmedTopic"] ?? "order.confirmed"))
        {
            var integrationEvent = JsonSerializer.Deserialize<OrderConfirmedIntegrationEvent>(payload, _serializerOptions)
                ?? throw new InvalidOperationException("Could not deserialize order.confirmed event.");

            var contact = await ResolveCustomerContactAsync(
                customerContactClient,
                integrationEvent.CustomerId,
                integrationEvent.CustomerEmail,
                cancellationToken);

            await QueueNotificationsIfNeededAsync(
                dbContext,
                new EmailNotification(
                    integrationEvent.OrderId,
                    integrationEvent.CustomerId,
                    topic,
                    nameof(OrderConfirmedIntegrationEvent),
                    contact.Email,
                    "Pedido confirmado",
                    $"Seu pedido {integrationEvent.OrderId} foi confirmado com sucesso. Total: {integrationEvent.TotalAmount:0.00} {integrationEvent.Currency.ToUpperInvariant()}.",
                    $"email:{topic}:{integrationEvent.OrderId}"),
                new WhatsAppNotification(
                    integrationEvent.OrderId,
                    integrationEvent.CustomerId,
                    topic,
                    nameof(OrderConfirmedIntegrationEvent),
                    contact.PhoneNumber,
                    $"Seu pedido {integrationEvent.OrderId} foi confirmado com sucesso.",
                    $"whatsapp:{topic}:{integrationEvent.OrderId}"),
                cancellationToken);

            return;
        }

        if (topic == (_configuration["Kafka:OrderRejectedTopic"] ?? "order.rejected"))
        {
            var integrationEvent = JsonSerializer.Deserialize<OrderRejectedIntegrationEvent>(payload, _serializerOptions)
                ?? throw new InvalidOperationException("Could not deserialize order.rejected event.");

            var contact = await ResolveCustomerContactAsync(customerContactClient, integrationEvent.CustomerId, null, cancellationToken);

            await QueueNotificationsIfNeededAsync(
                dbContext,
                new EmailNotification(
                    integrationEvent.OrderId,
                    integrationEvent.CustomerId,
                    topic,
                    nameof(OrderRejectedIntegrationEvent),
                    contact.Email,
                    "Pedido rejeitado",
                    $"Seu pedido {integrationEvent.OrderId} foi rejeitado. Motivo: {integrationEvent.Reason}.",
                    $"email:{topic}:{integrationEvent.OrderId}"),
                new WhatsAppNotification(
                    integrationEvent.OrderId,
                    integrationEvent.CustomerId,
                    topic,
                    nameof(OrderRejectedIntegrationEvent),
                    contact.PhoneNumber,
                    $"Seu pedido {integrationEvent.OrderId} foi rejeitado. Motivo: {integrationEvent.Reason}.",
                    $"whatsapp:{topic}:{integrationEvent.OrderId}"),
                cancellationToken);

            return;
        }

        var expeditionEvent = JsonSerializer.Deserialize<ExpeditionStatusChangedIntegrationEvent>(payload, _serializerOptions)
            ?? throw new InvalidOperationException("Could not deserialize expedition status event.");

        var expeditionContact = await ResolveCustomerContactAsync(customerContactClient, expeditionEvent.CustomerId, null, cancellationToken);
        var (emailSubject, emailBody, whatsAppMessage) = BuildExpeditionMessages(expeditionEvent);
        var eventKey = expeditionEvent.EventId == Guid.Empty
            ? $"{expeditionEvent.OrderId}:{expeditionEvent.Status}:{expeditionEvent.OccurredAtUtc:O}"
            : expeditionEvent.EventId.ToString();

        await QueueNotificationsIfNeededAsync(
            dbContext,
            new EmailNotification(
                expeditionEvent.OrderId,
                expeditionEvent.CustomerId,
                topic,
                nameof(ExpeditionStatusChangedIntegrationEvent),
                expeditionContact.Email,
                emailSubject,
                emailBody,
                $"email:{topic}:{eventKey}"),
            new WhatsAppNotification(
                expeditionEvent.OrderId,
                expeditionEvent.CustomerId,
                topic,
                nameof(ExpeditionStatusChangedIntegrationEvent),
                expeditionContact.PhoneNumber,
                whatsAppMessage,
                $"whatsapp:{topic}:{eventKey}"),
            cancellationToken);
    }

    private async Task QueueNotificationsIfNeededAsync(
        NotificationDbContext dbContext,
        EmailNotification emailNotification,
        WhatsAppNotification whatsAppNotification,
        CancellationToken cancellationToken)
    {
        var emailExists = await dbContext.EmailNotifications.AnyAsync(
            item => item.DeduplicationKey == emailNotification.DeduplicationKey,
            cancellationToken);

        if (!emailExists)
            await dbContext.EmailNotifications.AddAsync(emailNotification, cancellationToken);

        var whatsAppExists = await dbContext.WhatsAppNotifications.AnyAsync(
            item => item.DeduplicationKey == whatsAppNotification.DeduplicationKey,
            cancellationToken);

        if (!whatsAppExists)
            await dbContext.WhatsAppNotifications.AddAsync(whatsAppNotification, cancellationToken);
    }

    private async Task<CustomerContact> ResolveCustomerContactAsync(
        ICustomerContactClient customerContactClient,
        Guid customerId,
        string? fallbackEmail,
        CancellationToken cancellationToken)
    {
        var customer = await customerContactClient.GetByIdAsync(customerId, cancellationToken);

        var email = !string.IsNullOrWhiteSpace(customer?.Email)
            ? customer.Email
            : fallbackEmail ?? string.Empty;

        var phoneNumber = customer?.PhoneNumber ?? string.Empty;

        if (string.IsNullOrWhiteSpace(email))
            throw new InvalidOperationException($"Customer '{customerId}' does not have an email available for notification.");

        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new InvalidOperationException($"Customer '{customerId}' does not have a phone number available for notification.");

        return new CustomerContact(customerId, email, phoneNumber);
    }

    private string[] ResolveTopics() =>
        [
            _configuration["Kafka:OrderConfirmedTopic"] ?? "order.confirmed",
            _configuration["Kafka:OrderRejectedTopic"] ?? "order.rejected",
            _configuration["Kafka:ExpeditionAwaitingCarrierPickupTopic"] ?? "expedition.awaiting-carrier-pickup",
            _configuration["Kafka:ExpeditionPickedUpByCarrierTopic"] ?? "expedition.picked-up-by-carrier",
            _configuration["Kafka:ExpeditionInTransitTopic"] ?? "expedition.in-transit",
            _configuration["Kafka:ExpeditionDeliveredTopic"] ?? "expedition.delivered",
            _configuration["Kafka:ExpeditionDeliveryFailedTopic"] ?? "expedition.delivery-failed"
        ];

    private string ResolveMessageType(string topic)
    {
        if (topic == (_configuration["Kafka:OrderConfirmedTopic"] ?? "order.confirmed"))
            return nameof(OrderConfirmedIntegrationEvent);

        if (topic == (_configuration["Kafka:OrderRejectedTopic"] ?? "order.rejected"))
            return nameof(OrderRejectedIntegrationEvent);

        return nameof(ExpeditionStatusChangedIntegrationEvent);
    }

    private static (string Subject, string EmailBody, string WhatsAppMessage) BuildExpeditionMessages(ExpeditionStatusChangedIntegrationEvent integrationEvent)
    {
        return integrationEvent.Status switch
        {
            "AwaitingCarrierPickup" => (
                "Pedido aguardando coleta",
                $"Seu pedido {integrationEvent.OrderId} esta aguardando coleta da transportadora.",
                $"Seu pedido {integrationEvent.OrderId} esta aguardando coleta da transportadora."),
            "PickedUpByCarrier" => (
                "Pedido coletado pela transportadora",
                $"Seu pedido {integrationEvent.OrderId} foi coletado pela transportadora.",
                $"Seu pedido {integrationEvent.OrderId} foi coletado pela transportadora."),
            "InTransit" => (
                "Pedido em transporte",
                $"Seu pedido {integrationEvent.OrderId} esta em transporte.",
                $"Seu pedido {integrationEvent.OrderId} esta em transporte."),
            "Delivered" => (
                "Pedido entregue",
                $"Seu pedido {integrationEvent.OrderId} foi entregue com sucesso.",
                $"Seu pedido {integrationEvent.OrderId} foi entregue com sucesso."),
            "DeliveryFailed" => (
                "Falha na entrega do pedido",
                $"Nao foi possivel entregar o pedido {integrationEvent.OrderId}. Motivo: {integrationEvent.FailureReason}. {integrationEvent.FailureDetails}".Trim(),
                $"Nao foi possivel entregar o pedido {integrationEvent.OrderId}. Motivo: {integrationEvent.FailureReason}."),
            _ => (
                "Atualizacao do pedido",
                $"Seu pedido {integrationEvent.OrderId} recebeu uma atualizacao de expedicao: {integrationEvent.Status}.",
                $"Seu pedido {integrationEvent.OrderId} recebeu uma atualizacao: {integrationEvent.Status}.")
        };
    }
}
