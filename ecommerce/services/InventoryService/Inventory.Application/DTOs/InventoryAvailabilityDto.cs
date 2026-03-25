namespace Inventory.Application.DTOs;

public class InventoryAvailabilityDto
{
    public Guid ProductId { get; set; }
    public int AvailableQuantity { get; set; }
    public int ReservedQuantity { get; set; }
    public bool Active { get; set; }
}
