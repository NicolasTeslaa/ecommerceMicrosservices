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
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(10);

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
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var bootstrapServers = _configuration["Kafka:BootstrapServers"];
                if (string.IsNullOrWhiteSpace(bootstrapServers))
                {
                    _logger.LogWarning("Notification consumer is waiting because Kafka:BootstrapServers was not configured.");
                    await Task.Delay(RetryDelay, stoppingToken);
                    continue;
                }

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
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Unexpected error while consuming notification topics.");
                await Task.Delay(RetryDelay, stoppingToken);
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
            var integrationEvent = JsonSerializer.Deserialize<OrderConfirmedIntegrationEvent>(payload, _serializerOptions);
            if (integrationEvent is null)
            {
                _logger.LogWarning("Notification consumer ignored an order.confirmed message because it could not deserialize the payload.");
                return;
            }

            var contact = await ResolveCustomerContactAsync(
                customerContactClient,
                integrationEvent.CustomerId,
                integrationEvent.CustomerEmail,
                cancellationToken);

            await QueueNotificationsIfNeededAsync(
                dbContext,
                !string.IsNullOrWhiteSpace(contact.Email)
                    ? new EmailNotification(
                        integrationEvent.OrderId,
                        integrationEvent.CustomerId,
                        topic,
                        nameof(OrderConfirmedIntegrationEvent),
                        contact.Email,
                        "Pedido confirmado",
                        $"Seu pedido {integrationEvent.OrderId} foi confirmado com sucesso. Total: {integrationEvent.TotalAmount:0.00} {integrationEvent.Currency.ToUpperInvariant()}.",
                        $"email:{topic}:{integrationEvent.OrderId}")
                    : null,
                !string.IsNullOrWhiteSpace(contact.PhoneNumber)
                    ? new WhatsAppNotification(
                        integrationEvent.OrderId,
                        integrationEvent.CustomerId,
                        topic,
                        nameof(OrderConfirmedIntegrationEvent),
                        contact.PhoneNumber,
                        $"Seu pedido {integrationEvent.OrderId} foi confirmado com sucesso.",
                        $"whatsapp:{topic}:{integrationEvent.OrderId}")
                    : null,
                cancellationToken);

            return;
        }

        if (topic == (_configuration["Kafka:OrderRejectedTopic"] ?? "order.rejected"))
        {
            var integrationEvent = JsonSerializer.Deserialize<OrderRejectedIntegrationEvent>(payload, _serializerOptions);
            if (integrationEvent is null)
            {
                _logger.LogWarning("Notification consumer ignored an order.rejected message because it could not deserialize the payload.");
                return;
            }

            var contact = await ResolveCustomerContactAsync(customerContactClient, integrationEvent.CustomerId, null, cancellationToken);

            await QueueNotificationsIfNeededAsync(
                dbContext,
                !string.IsNullOrWhiteSpace(contact.Email)
                    ? new EmailNotification(
                        integrationEvent.OrderId,
                        integrationEvent.CustomerId,
                        topic,
                        nameof(OrderRejectedIntegrationEvent),
                        contact.Email,
                        "Pedido rejeitado",
                        $"Seu pedido {integrationEvent.OrderId} foi rejeitado. Motivo: {integrationEvent.Reason}.",
                        $"email:{topic}:{integrationEvent.OrderId}")
                    : null,
                !string.IsNullOrWhiteSpace(contact.PhoneNumber)
                    ? new WhatsAppNotification(
                        integrationEvent.OrderId,
                        integrationEvent.CustomerId,
                        topic,
                        nameof(OrderRejectedIntegrationEvent),
                        contact.PhoneNumber,
                        $"Seu pedido {integrationEvent.OrderId} foi rejeitado. Motivo: {integrationEvent.Reason}.",
                        $"whatsapp:{topic}:{integrationEvent.OrderId}")
                    : null,
                cancellationToken);

            return;
        }

        var expeditionEvent = JsonSerializer.Deserialize<ExpeditionStatusChangedIntegrationEvent>(payload, _serializerOptions);
        if (expeditionEvent is null)
        {
            _logger.LogWarning("Notification consumer ignored an expedition status message because it could not deserialize the payload.");
            return;
        }

        var expeditionContact = await ResolveCustomerContactAsync(customerContactClient, expeditionEvent.CustomerId, null, cancellationToken);
        var (emailSubject, emailBody, whatsAppMessage) = BuildExpeditionMessages(expeditionEvent);
        var eventKey = expeditionEvent.EventId == Guid.Empty
            ? $"{expeditionEvent.OrderId}:{expeditionEvent.Status}:{expeditionEvent.OccurredAtUtc:O}"
            : expeditionEvent.EventId.ToString();

        await QueueNotificationsIfNeededAsync(
            dbContext,
            !string.IsNullOrWhiteSpace(expeditionContact.Email)
                ? new EmailNotification(
                    expeditionEvent.OrderId,
                    expeditionEvent.CustomerId,
                    topic,
                    nameof(ExpeditionStatusChangedIntegrationEvent),
                    expeditionContact.Email,
                    emailSubject,
                    emailBody,
                    $"email:{topic}:{eventKey}")
                : null,
            !string.IsNullOrWhiteSpace(expeditionContact.PhoneNumber)
                ? new WhatsAppNotification(
                    expeditionEvent.OrderId,
                    expeditionEvent.CustomerId,
                    topic,
                    nameof(ExpeditionStatusChangedIntegrationEvent),
                    expeditionContact.PhoneNumber,
                    whatsAppMessage,
                    $"whatsapp:{topic}:{eventKey}")
                : null,
            cancellationToken);
    }

    private async Task QueueNotificationsIfNeededAsync(
        NotificationDbContext dbContext,
        EmailNotification? emailNotification,
        WhatsAppNotification? whatsAppNotification,
        CancellationToken cancellationToken)
    {
        if (emailNotification is not null)
        {
            var emailExists = await dbContext.EmailNotifications.AnyAsync(
                item => item.DeduplicationKey == emailNotification.DeduplicationKey,
                cancellationToken);

            if (!emailExists && !string.IsNullOrWhiteSpace(emailNotification.RecipientEmail))
                await dbContext.EmailNotifications.AddAsync(emailNotification, cancellationToken);
        }

        if (whatsAppNotification is not null)
        {
            var whatsAppExists = await dbContext.WhatsAppNotifications.AnyAsync(
                item => item.DeduplicationKey == whatsAppNotification.DeduplicationKey,
                cancellationToken);

            if (!whatsAppExists && !string.IsNullOrWhiteSpace(whatsAppNotification.RecipientPhoneNumber))
                await dbContext.WhatsAppNotifications.AddAsync(whatsAppNotification, cancellationToken);
        }
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
        {
            _logger.LogWarning("Notification for customer '{CustomerId}' was skipped because no email was available.", customerId);
            return new CustomerContact(customerId, string.Empty, phoneNumber);
        }

        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            _logger.LogWarning("Notification for customer '{CustomerId}' was skipped because no phone number was available.", customerId);
            return new CustomerContact(customerId, email, string.Empty);
        }

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
