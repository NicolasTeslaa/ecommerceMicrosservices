using Order.Domain.Enums;

namespace Order.Application.DTOs;

public class OrderProcessingRequestDto
{
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public Guid CustomerAddressId { get; set; }
    public decimal ShippingAmount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? PaymentToken { get; set; }
    public string? PaymentCardBrand { get; set; }
    public string? PaymentCardLast4 { get; set; }
    public DateTime RequestedAtUtc { get; set; }
    public IReadOnlyCollection<OrderProcessingItemDto> Items { get; set; } = Array.Empty<OrderProcessingItemDto>();
}
