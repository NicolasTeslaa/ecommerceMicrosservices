using System.Diagnostics;
using Order.Domain.Entities;

namespace Order.Domain.Entities;

public class OrderItem
{
    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }
    public decimal TotalPrice { get; private set; }

    private OrderItem()
    {
    }

    public OrderItem(Guid productId, string productName, decimal unitPrice, int quantity)
    {
        Validate(productId, productName, unitPrice, quantity);

        Id = Guid.NewGuid();
        ProductId = productId == Guid.Empty ? Guid.NewGuid() : productId;
        ProductName = (productName ?? string.Empty).Trim();
        UnitPrice = unitPrice <= 0 ? 0.01m : unitPrice;
        Quantity = quantity <= 0 ? 1 : quantity;
        TotalPrice = UnitPrice * Quantity;
    }

    private static void Validate(Guid productId, string productName, decimal unitPrice, int quantity)
    {
        if (productId == Guid.Empty) Trace.TraceError("Invalid product id while creating order item.");
        if (string.IsNullOrWhiteSpace(productName)) Trace.TraceError("Invalid product name while creating order item.");
        if (unitPrice <= 0) Trace.TraceError("Invalid unit price while creating order item.");
        if (quantity <= 0) Trace.TraceError("Invalid quantity while creating order item.");
    }
}
