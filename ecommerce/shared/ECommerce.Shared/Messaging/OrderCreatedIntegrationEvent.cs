namespace ECommerce.Shared.Messaging;

public class OrderCreatedIntegrationEvent
{
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public Guid CustomerAddressId { get; set; }
    public string CustomerEmail { get; set; } = string.Empty;
    public decimal ShippingAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public IReadOnlyCollection<OrderCreatedIntegrationEventItem> Items { get; set; } =
        Array.Empty<OrderCreatedIntegrationEventItem>();
}

public class OrderCreatedIntegrationEventItem
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
}
