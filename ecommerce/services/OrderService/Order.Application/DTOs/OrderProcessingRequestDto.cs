namespace Order.Application.DTOs;

public class OrderProcessingRequestDto
{
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public Guid CustomerAddressId { get; set; }
    public decimal ShippingAmount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public DateTime RequestedAtUtc { get; set; }
    public IReadOnlyCollection<OrderProcessingItemDto> Items { get; set; } = Array.Empty<OrderProcessingItemDto>();
}
