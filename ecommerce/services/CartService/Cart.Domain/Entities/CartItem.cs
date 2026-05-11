using System.Diagnostics;

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
        {
            LogSoftFailure("CartItem received a non-positive quantity increment.");
            return;
        }

        Quantity += quantity;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void UpdateSnapshot(string productName, decimal unitPrice)
    {
        if (string.IsNullOrWhiteSpace(productName))
        {
            LogSoftFailure("CartItem received an empty product name.");
            return;
        }

        if (unitPrice <= 0)
        {
            LogSoftFailure("CartItem received a non-positive unit price.");
            return;
        }

        ProductName = productName.Trim();
        UnitPrice = unitPrice;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void SetQuantity(int quantity)
    {
        if (quantity <= 0)
        {
            LogSoftFailure("CartItem received a non-positive quantity.");
            return;
        }

        Quantity = quantity;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    private static void Validate(Guid productId, string productName, decimal unitPrice, int quantity)
    {
        if (productId == Guid.Empty)
            LogSoftFailure("CartItem received an empty product id.");

        if (string.IsNullOrWhiteSpace(productName))
            LogSoftFailure("CartItem received an empty product name.");

        if (unitPrice <= 0)
            LogSoftFailure("CartItem received a non-positive unit price.");

        if (quantity <= 0)
            LogSoftFailure("CartItem received a non-positive quantity.");
    }

    private static void LogSoftFailure(string message) => Trace.TraceError(message);
}
