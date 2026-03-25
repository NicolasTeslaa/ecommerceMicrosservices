namespace ECommerce.Shared.Messaging;

public class InventoryReservationRejectedIntegrationEvent
{
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public IReadOnlyCollection<InventoryReservationRejectedItemIntegrationEvent> Items { get; set; } =
        Array.Empty<InventoryReservationRejectedItemIntegrationEvent>();
    public DateTime RejectedAtUtc { get; set; }
}

public class InventoryReservationRejectedItemIntegrationEvent
{
    public Guid ProductId { get; set; }
    public int RequestedQuantity { get; set; }
    public int AvailableQuantity { get; set; }
    public string Reason { get; set; } = string.Empty;
}
