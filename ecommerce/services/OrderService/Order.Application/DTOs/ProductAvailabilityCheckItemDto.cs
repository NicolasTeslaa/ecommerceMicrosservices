namespace Order.Application.DTOs;

public class ProductAvailabilityCheckItemDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int RequestedQuantity { get; set; }
}
