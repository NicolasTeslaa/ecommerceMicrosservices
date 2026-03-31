using ECommerce.Shared.Messaging;
using Inventory.Application.Interfaces;
using Inventory.Domain.Entities;
using Inventory.Domain.Enums;
using Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Messaging;

public class PaymentApprovedMessageProcessor
{
    private readonly InventoryDbContext _dbContext;
    private readonly IInventoryRepository _repository;

    public PaymentApprovedMessageProcessor(InventoryDbContext dbContext, IInventoryRepository repository)
    {
        _dbContext = dbContext;
        _repository = repository;
    }

    public async Task<bool> ProcessAsync(
        PaymentApprovedIntegrationEvent integrationEvent,
        string topic,
        int partition,
        long offset,
        string groupId,
        CancellationToken cancellationToken = default)
    {
        var alreadyProcessed = await _dbContext.ProcessedKafkaMessages.AnyAsync(
            item => item.Topic == topic
                && item.Partition == partition
                && item.Offset == offset,
            cancellationToken);

        if (alreadyProcessed)
            return true;

        var reservations = await _repository.GetReservationsByOrderIdAsync(integrationEvent.OrderId, cancellationToken);
        var items = await _repository.GetItemsByProductIdsAsync(
            reservations.Select(item => item.ProductId).Distinct().ToArray(),
            cancellationToken);
        var itemsByProductId = items.ToDictionary(item => item.ProductId);

        foreach (var reservation in reservations.Where(item => item.Status == InventoryReservationStatus.Pending))
        {
            if (!itemsByProductId.TryGetValue(reservation.ProductId, out var inventoryItem))
                continue;

            inventoryItem.ConfirmReservation(reservation.Quantity);
            reservation.Confirm();
        }

        await _dbContext.ProcessedKafkaMessages.AddAsync(
            new ProcessedKafkaMessage(topic, partition, offset, groupId),
            cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        return true;
    }
}
