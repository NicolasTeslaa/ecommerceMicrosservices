namespace Inventory.Application.DTOs;

public class InventoryReservationIssueDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int RequestedQuantity { get; set; }
    public int AvailableQuantity { get; set; }
    public string Reason { get; set; } = string.Empty;
}
