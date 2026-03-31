namespace ECommerce.Shared.Messaging;

public class OrderConfirmedIntegrationEvent
{
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public Guid CustomerAddressId { get; set; }
    public string CustomerEmail { get; set; } = string.Empty;
    public decimal ShippingAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "brl";
    public string Status { get; set; } = "Confirmed";
    public DateTime ConfirmedAtUtc { get; set; }
    public OrderConfirmedIntegrationEventItem[] Items { get; set; } = [];
}

public class OrderConfirmedIntegrationEventItem
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
}
