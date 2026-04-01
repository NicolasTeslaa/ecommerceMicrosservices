using System.Text.Json;
using ECommerce.Shared.Messaging;
using Expedition.Application.Interfaces;
using Expedition.Domain.Entities;
using Expedition.Domain.Enums;
using Expedition.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Expedition.Infrastructure.Messaging;

public class KafkaExpeditionEventPublisher : IExpeditionEventPublisher
{
    private readonly IConfiguration _configuration;
    private readonly ExpeditionDbContext _dbContext;

    public KafkaExpeditionEventPublisher(IConfiguration configuration, ExpeditionDbContext dbContext)
    {
        _configuration = configuration;
        _dbContext = dbContext;
    }

    public async Task PublishStatusChangedAsync(ExpeditionOrder expeditionOrder, CancellationToken cancellationToken = default)
    {
        var deduplicationKey = $"{expeditionOrder.OrderId}:{expeditionOrder.Status}";

        var alreadyQueued = _dbContext.ExpeditionOutboxMessages.Local
            .Any(message => message.DeduplicationKey == deduplicationKey)
            || await _dbContext.ExpeditionOutboxMessages
                .AnyAsync(message => message.DeduplicationKey == deduplicationKey, cancellationToken);

        if (alreadyQueued)
            return;

        var topic = ResolveTopic(expeditionOrder.Status);
        var integrationEvent = new ExpeditionStatusChangedIntegrationEvent
        {
            EventId = Guid.NewGuid(),
            ExpeditionId = expeditionOrder.Id,
            OrderId = expeditionOrder.OrderId,
            InvoiceId = expeditionOrder.InvoiceId,
            CustomerId = expeditionOrder.CustomerId,
            InvoiceNumber = expeditionOrder.InvoiceNumber,
            InvoiceSeries = expeditionOrder.InvoiceSeries,
            InvoiceAccessKey = expeditionOrder.InvoiceAccessKey,
            Status = expeditionOrder.Status.ToString(),
            FailureReason = expeditionOrder.FailureReason.ToString(),
            FailureDetails = expeditionOrder.FailureDetails,
            OccurredAtUtc = ResolveOccurredAtUtc(expeditionOrder),
            CreatedAtUtc = expeditionOrder.CreatedAtUtc,
            UpdatedAtUtc = expeditionOrder.UpdatedAtUtc,
            PickedUpAtUtc = expeditionOrder.PickedUpAtUtc,
            InTransitAtUtc = expeditionOrder.InTransitAtUtc,
            DeliveredAtUtc = expeditionOrder.DeliveredAtUtc,
            FailedAtUtc = expeditionOrder.FailedAtUtc
        };

        var message = ExpeditionOutboxMessage.Create(
            expeditionOrder.Id,
            topic,
            expeditionOrder.OrderId.ToString(),
            nameof(ExpeditionStatusChangedIntegrationEvent),
            JsonSerializer.Serialize(integrationEvent),
            deduplicationKey);

        await _dbContext.ExpeditionOutboxMessages.AddAsync(message, cancellationToken);
    }

    private string ResolveTopic(ExpeditionStatus status)
    {
        return status switch
        {
            ExpeditionStatus.AwaitingCarrierPickup => _configuration["Kafka:ExpeditionAwaitingCarrierPickupTopic"] ?? "expedition.awaiting-carrier-pickup",
            ExpeditionStatus.PickedUpByCarrier => _configuration["Kafka:ExpeditionPickedUpByCarrierTopic"] ?? "expedition.picked-up-by-carrier",
            ExpeditionStatus.InTransit => _configuration["Kafka:ExpeditionInTransitTopic"] ?? "expedition.in-transit",
            ExpeditionStatus.Delivered => _configuration["Kafka:ExpeditionDeliveredTopic"] ?? "expedition.delivered",
            ExpeditionStatus.DeliveryFailed => _configuration["Kafka:ExpeditionDeliveryFailedTopic"] ?? "expedition.delivery-failed",
            _ => throw new InvalidOperationException($"Unsupported expedition status '{status}'.")
        };
    }

    private static DateTime ResolveOccurredAtUtc(ExpeditionOrder expeditionOrder)
    {
        return expeditionOrder.Status switch
        {
            ExpeditionStatus.AwaitingCarrierPickup => expeditionOrder.CreatedAtUtc,
            ExpeditionStatus.PickedUpByCarrier => expeditionOrder.PickedUpAtUtc ?? expeditionOrder.UpdatedAtUtc,
            ExpeditionStatus.InTransit => expeditionOrder.InTransitAtUtc ?? expeditionOrder.UpdatedAtUtc,
            ExpeditionStatus.Delivered => expeditionOrder.DeliveredAtUtc ?? expeditionOrder.UpdatedAtUtc,
            ExpeditionStatus.DeliveryFailed => expeditionOrder.FailedAtUtc ?? expeditionOrder.UpdatedAtUtc,
            _ => expeditionOrder.UpdatedAtUtc
        };
    }
}
