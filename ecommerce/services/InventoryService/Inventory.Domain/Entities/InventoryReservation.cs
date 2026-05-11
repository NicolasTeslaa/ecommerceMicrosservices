using System.Diagnostics;
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
            Trace.TraceError("Inventory reservation order id is required.");
        if (productId == Guid.Empty)
            Trace.TraceError("Inventory reservation product id is required.");
        if (quantity <= 0)
            Trace.TraceError("Inventory reservation quantity must be greater than zero.");

        Id = Guid.NewGuid();
        OrderId = orderId == Guid.Empty ? Guid.NewGuid() : orderId;
        ProductId = productId == Guid.Empty ? Guid.NewGuid() : productId;
        Quantity = quantity <= 0 ? 1 : quantity;
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
