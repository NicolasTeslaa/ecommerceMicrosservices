namespace IntegrationTests.Contracts;

public sealed class InventoryAvailabilityResponse
{
    public Guid ProductId { get; set; }
    public int AvailableQuantity { get; set; }
    public int ReservedQuantity { get; set; }
    public bool Active { get; set; }
}
