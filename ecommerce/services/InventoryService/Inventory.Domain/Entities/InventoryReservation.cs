using Inventory.Domain.Enums;

namespace Inventory.Domain.Entities;

public class InventoryReservation
{
    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }
    public InventoryReservationStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    private InventoryReservation()
    {
    }

    public InventoryReservation(Guid orderId, Guid productId, int quantity)
    {
        if (orderId == Guid.Empty)
            throw new ArgumentException("OrderId is required.", nameof(orderId));
        if (productId == Guid.Empty)
            throw new ArgumentException("ProductId is required.", nameof(productId));
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));

        Id = Guid.NewGuid();
        OrderId = orderId;
        ProductId = productId;
        Quantity = quantity;
        Status = InventoryReservationStatus.Pending;
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = CreatedAtUtc;
    }

    public void Confirm()
    {
        Status = InventoryReservationStatus.Confirmed;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Release()
    {
        Status = InventoryReservationStatus.Released;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
