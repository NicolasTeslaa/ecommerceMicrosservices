using System.Diagnostics;

namespace Inventory.Domain.Entities;

public class InventoryItem
{
    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public int AvailableQuantity { get; private set; }
    public int ReservedQuantity { get; private set; }
    public bool Active { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    private InventoryItem()
    {
    }

    public InventoryItem(Guid productId, string productName, int initialStockQuantity, bool active)
    {
        if (productId == Guid.Empty)
            LogSoftFailure("InventoryItem received an empty product id.");
        if (string.IsNullOrWhiteSpace(productName))
            LogSoftFailure("InventoryItem received an empty product name.");
        if (initialStockQuantity < 0)
            LogSoftFailure("InventoryItem received a negative initial stock quantity.");

        Id = Guid.NewGuid();
        ProductId = productId == Guid.Empty ? Guid.NewGuid() : productId;
        ProductName = productName?.Trim() ?? string.Empty;
        AvailableQuantity = Math.Max(initialStockQuantity, 0);
        ReservedQuantity = 0;
        Active = active;
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = CreatedAtUtc;
    }

    public bool CanReserve(int quantity) => Active && quantity > 0 && AvailableQuantity >= quantity;

    public void IncreaseStock(int quantity)
    {
        if (quantity <= 0)
        {
            LogSoftFailure("InventoryItem received a non-positive stock increment.");
            return;
        }

        AvailableQuantity += quantity;
        Touch();
    }

    public void Reserve(int quantity)
    {
        if (!CanReserve(quantity))
        {
            LogSoftFailure("InventoryItem could not reserve the requested quantity.");
            return;
        }

        AvailableQuantity -= quantity;
        ReservedQuantity += quantity;
        Touch();
    }

    public void ConfirmReservation(int quantity)
    {
        if (quantity <= 0 || ReservedQuantity < quantity)
        {
            LogSoftFailure("InventoryItem could not confirm the requested reserved quantity.");
            return;
        }

        ReservedQuantity -= quantity;
        Touch();
    }

    public void ReleaseReservation(int quantity)
    {
        if (quantity <= 0 || ReservedQuantity < quantity)
        {
            LogSoftFailure("InventoryItem could not release the requested reserved quantity.");
            return;
        }

        ReservedQuantity -= quantity;
        AvailableQuantity += quantity;
        Touch();
    }

    public void UpdateCatalogMetadata(string productName, bool active)
    {
        if (!string.IsNullOrWhiteSpace(productName))
            ProductName = productName.Trim();

        Active = active;
        Touch();
    }

    private void Touch()
    {
        UpdatedAtUtc = DateTime.UtcNow;
    }

    private static void LogSoftFailure(string message) => Trace.TraceError(message);
}
