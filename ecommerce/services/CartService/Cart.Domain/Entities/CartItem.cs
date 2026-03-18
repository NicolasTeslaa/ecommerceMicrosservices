using Cart.Domain.Exceptions;

namespace Cart.Domain.Entities;

public class CartItem
{
    public Guid Id { get; private set; }
    public Guid CartId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public decimal Subtotal => UnitPrice * Quantity;

    private CartItem()
    {
    }

    public CartItem(Guid productId, string productName, decimal unitPrice, int quantity)
    {
        Validate(productId, productName, unitPrice, quantity);

        Id = Guid.NewGuid();
        ProductId = productId;
        ProductName = productName.Trim();
        UnitPrice = unitPrice;
        Quantity = quantity;
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = CreatedAtUtc;
    }

    public void IncreaseQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new InvalidQuantityException();

        Quantity += quantity;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void UpdateSnapshot(string productName, decimal unitPrice)
    {
        if (string.IsNullOrWhiteSpace(productName))
            throw new InvalidProductNameException();

        if (unitPrice <= 0)
            throw new InvalidUnitPriceException();

        ProductName = productName.Trim();
        UnitPrice = unitPrice;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void SetQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new InvalidQuantityException();

        Quantity = quantity;
        UpdatedAtUtc = DateTime.UtcNow;
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
