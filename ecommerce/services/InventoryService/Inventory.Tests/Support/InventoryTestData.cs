using Inventory.Application.DTOs;
using Inventory.Domain.Entities;

namespace Inventory.Tests.Support;

public static class InventoryTestData
{
    public static InventoryItem CreateItem(
        Guid? productId = null,
        string productName = "Produto teste",
        int initialStockQuantity = 10,
        bool active = true)
    {
        return new InventoryItem(productId ?? Guid.NewGuid(), productName, initialStockQuantity, active);
    }

    public static InventoryReservation CreateReservation(
        Guid? orderId = null,
        Guid? productId = null,
        int quantity = 2)
    {
        return new InventoryReservation(orderId ?? Guid.NewGuid(), productId ?? Guid.NewGuid(), quantity);
    }

    public static InventoryAvailabilityDto CreateAvailabilityDto(Guid? productId = null)
    {
        return new InventoryAvailabilityDto
        {
            ProductId = productId ?? Guid.NewGuid(),
            AvailableQuantity = 10,
            ReservedQuantity = 2,
            Active = true
        };
    }
}
