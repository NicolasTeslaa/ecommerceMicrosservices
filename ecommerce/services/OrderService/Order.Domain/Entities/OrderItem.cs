using Order.Domain.Exceptions;

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
        ProductId = productId;
        ProductName = productName.Trim();
        UnitPrice = unitPrice;
        Quantity = quantity;
        TotalPrice = unitPrice * quantity;
    }

    private static void Validate(Guid productId, string productName, decimal unitPrice, int quantity)
    {
        if (productId == Guid.Empty)
            throw new InvalidProductIdException();

        if (string.IsNullOrWhiteSpace(productName))
            throw new InvalidProductNameException();

        if (unitPrice <= 0)
            throw new InvalidUnitPriceException();

        if (quantity <= 0)
            throw new InvalidQuantityException();
    }
}
