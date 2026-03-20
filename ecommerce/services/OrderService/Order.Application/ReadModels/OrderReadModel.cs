using Order.Domain.Enums;

namespace Order.Application.ReadModels;

public class OrderReadModel
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public Guid CustomerAddressId { get; set; }
    public string CustomerEmail { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;
    public decimal ShippingAmount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? PaymentCardBrand { get; set; }
    public string? PaymentCardLast4 { get; set; }
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; }
    public OrderRejectionReason? RejectionReason { get; set; }
    public string? RejectionDetail { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public List<OrderItemReadModel> Items { get; set; } = new();
}
