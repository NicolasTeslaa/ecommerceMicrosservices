namespace ECommerce.Shared.Messaging;

public class CatalogProductCreatedIntegrationEvent
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int InitialStockQuantity { get; set; }
    public bool Active { get; set; }
    public DateTime OccurredAtUtc { get; set; }
}
