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
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public List<OrderItemReadModel> Items { get; set; } = new();
}
