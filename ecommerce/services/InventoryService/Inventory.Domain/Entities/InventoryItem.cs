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
            throw new ArgumentException("ProductId is required.", nameof(productId));
        if (string.IsNullOrWhiteSpace(productName))
            throw new ArgumentException("ProductName is required.", nameof(productName));
        if (initialStockQuantity < 0)
            throw new ArgumentException("Initial stock quantity cannot be negative.", nameof(initialStockQuantity));

        Id = Guid.NewGuid();
        ProductId = productId;
        ProductName = productName.Trim();
        AvailableQuantity = initialStockQuantity;
        ReservedQuantity = 0;
        Active = active;
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = CreatedAtUtc;
    }

    public bool CanReserve(int quantity) => Active && quantity > 0 && AvailableQuantity >= quantity;

    public void IncreaseStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));

        AvailableQuantity += quantity;
        Touch();
    }

    public void Reserve(int quantity)
    {
        if (!CanReserve(quantity))
            throw new InvalidOperationException("Insufficient inventory to reserve.");

        AvailableQuantity -= quantity;
        ReservedQuantity += quantity;
        Touch();
    }

    public void ConfirmReservation(int quantity)
    {
        if (quantity <= 0 || ReservedQuantity < quantity)
            throw new InvalidOperationException("Reserved inventory is insufficient to confirm.");

        ReservedQuantity -= quantity;
        Touch();
    }

    public void ReleaseReservation(int quantity)
    {
        if (quantity <= 0 || ReservedQuantity < quantity)
            throw new InvalidOperationException("Reserved inventory is insufficient to release.");

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
}
