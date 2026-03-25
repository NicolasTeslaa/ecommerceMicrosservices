using ECommerce.Shared.Protos;
using Inventory.Application.DTOs;
using Inventory.Application.Interfaces;
using Inventory.Domain.Entities;
using Inventory.Domain.Enums;
using Grpc.Core;

namespace Inventory.Infrastructure.Grpc;

public class InventoryOrderReservationGrpcService : InventoryOrderReservation.InventoryOrderReservationBase
{
    private readonly IInventoryRepository _repository;
    private readonly IInventoryEventPublisher _eventPublisher;

    public InventoryOrderReservationGrpcService(IInventoryRepository repository, IInventoryEventPublisher eventPublisher)
    {
        _repository = repository;
        _eventPublisher = eventPublisher;
    }

    public override async Task<ReserveOrderItemsReply> ReserveOrderItems(ReserveOrderItemsRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.OrderId, out var orderId) || !Guid.TryParse(request.CustomerId, out var customerId))
        {
            return new ReserveOrderItemsReply
            {
                IsSuccess = false,
                Reason = "Order or customer identifier is invalid."
            };
        }

        var existingReservations = await _repository.GetReservationsByOrderIdAsync(orderId, context.CancellationToken);
        if (existingReservations.Any())
        {
            return new ReserveOrderItemsReply
            {
                IsSuccess = true,
                Reason = "Inventory was already reserved for this order."
            };
        }

        var requestedItems = request.Items
            .Where(item => Guid.TryParse(item.ProductId, out _))
            .GroupBy(item => Guid.Parse(item.ProductId))
            .Select(group => new
            {
                ProductId = group.Key,
                ProductName = group.Select(item => item.ProductName).FirstOrDefault(name => !string.IsNullOrWhiteSpace(name)) ?? "Produto",
                RequestedQuantity = group.Sum(item => item.Quantity)
            })
            .ToArray();

        var inventoryItems = await _repository.GetItemsByProductIdsAsync(requestedItems.Select(item => item.ProductId).ToArray(), context.CancellationToken);
        var inventoryByProductId = inventoryItems.ToDictionary(item => item.ProductId);
        var issues = new List<InventoryReservationIssueDto>();

        foreach (var requestedItem in requestedItems)
        {
            if (!inventoryByProductId.TryGetValue(requestedItem.ProductId, out var inventoryItem) || !inventoryItem.Active)
            {
                issues.Add(new InventoryReservationIssueDto
                {
                    ProductId = requestedItem.ProductId,
                    ProductName = requestedItem.ProductName,
                    RequestedQuantity = requestedItem.RequestedQuantity,
                    AvailableQuantity = 0,
                    Reason = "Product does not exist or is inactive."
                });
                continue;
            }

            if (!inventoryItem.CanReserve(requestedItem.RequestedQuantity))
            {
                issues.Add(new InventoryReservationIssueDto
                {
                    ProductId = requestedItem.ProductId,
                    ProductName = inventoryItem.ProductName,
                    RequestedQuantity = requestedItem.RequestedQuantity,
                    AvailableQuantity = inventoryItem.AvailableQuantity,
                    Reason = "Insufficient stock."
                });
            }
        }

        if (issues.Count > 0)
        {
            await _eventPublisher.PublishReservationRejectedAsync(
                orderId,
                customerId,
                issues.Count == 1 ? issues[0].Reason : "One or more products are unavailable for reservation.",
                issues,
                context.CancellationToken);

            var reply = new ReserveOrderItemsReply
            {
                IsSuccess = false,
                Reason = issues.Count == 1 ? issues[0].Reason : "One or more products are unavailable for reservation."
            };
            reply.RejectedItems.AddRange(issues.Select(issue => new ReserveOrderRejectedItem
            {
                ProductId = issue.ProductId.ToString(),
                ProductName = issue.ProductName,
                RequestedQuantity = issue.RequestedQuantity,
                AvailableQuantity = issue.AvailableQuantity,
                Reason = issue.Reason
            }));

            return reply;
        }

        var reservations = new List<InventoryReservation>();

        foreach (var requestedItem in requestedItems)
        {
            var inventoryItem = inventoryByProductId[requestedItem.ProductId];
            inventoryItem.Reserve(requestedItem.RequestedQuantity);
            reservations.Add(new InventoryReservation(orderId, requestedItem.ProductId, requestedItem.RequestedQuantity));
        }

        await _repository.AddReservationsAsync(reservations, context.CancellationToken);
        await _repository.SaveChangesAsync(context.CancellationToken);

        return new ReserveOrderItemsReply
        {
            IsSuccess = true,
            Reason = "Inventory reserved successfully."
        };
    }

    public override async Task<ReleaseOrderReservationReply> ReleaseOrderReservation(ReleaseOrderReservationRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.OrderId, out var orderId))
        {
            return new ReleaseOrderReservationReply
            {
                IsSuccess = false,
                Reason = "Order identifier is invalid."
            };
        }

        var reservations = await _repository.GetReservationsByOrderIdAsync(orderId, context.CancellationToken);
        if (!reservations.Any())
        {
            return new ReleaseOrderReservationReply
            {
                IsSuccess = true,
                Reason = "No reservations were found for this order."
            };
        }

        var items = await _repository.GetItemsByProductIdsAsync(
            reservations.Select(item => item.ProductId).Distinct().ToArray(),
            context.CancellationToken);
        var itemsByProductId = items.ToDictionary(item => item.ProductId);

        foreach (var reservation in reservations.Where(item => item.Status == InventoryReservationStatus.Pending))
        {
            if (!itemsByProductId.TryGetValue(reservation.ProductId, out var inventoryItem))
                continue;

            inventoryItem.ReleaseReservation(reservation.Quantity);
            reservation.Release();
        }

        await _repository.SaveChangesAsync(context.CancellationToken);

        return new ReleaseOrderReservationReply
        {
            IsSuccess = true,
            Reason = "Inventory reservations released successfully."
        };
    }
}
