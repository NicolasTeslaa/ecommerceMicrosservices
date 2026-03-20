namespace ECommerce.Shared.Messaging;

public class OrderRejectedIntegrationEvent
{
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public Guid CustomerAddressId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime RequestedAtUtc { get; set; }
    public DateTime RejectedAtUtc { get; set; }
    public IReadOnlyCollection<OrderRejectedIntegrationEventItem> Items { get; set; } =
        Array.Empty<OrderRejectedIntegrationEventItem>();
}

public class OrderRejectedIntegrationEventItem
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int RequestedQuantity { get; set; }
    public int AvailableQuantity { get; set; }
    public string Reason { get; set; } = string.Empty;
}
