using System.Diagnostics;
using Cart.Domain.Enums;

namespace Cart.Domain.Entities;

public class Cart
{
    public Guid Id { get; private set; }
    public string OwnerId { get; private set; } = string.Empty;
    public CartOwnerType OwnerType { get; private set; }
    public CartStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public ICollection<CartItem> Items { get; private set; } = new List<CartItem>();
    public decimal TotalAmount => Items.Sum(item => item.Subtotal);

    private Cart()
    {
    }

    public Cart(string ownerId, CartOwnerType ownerType)
    {
        ValidateOwner(ownerId, ownerType);

        Id = Guid.NewGuid();
        OwnerId = (ownerId ?? string.Empty).Trim();
        OwnerType = ownerType;
        Status = CartStatus.Active;
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = CreatedAtUtc;
    }

    public void AddItem(Guid productId, string productName, decimal unitPrice, int quantity)
    {
        if (quantity <= 0)
        {
            Trace.TraceError("Invalid quantity while adding item to cart.");
            return;
        }

        var existingItem = Items.FirstOrDefault(item => item.ProductId == productId);

        if (existingItem is null)
        {
            Items.Add(new CartItem(productId, productName, unitPrice, quantity));
        }
        else
        {
            existingItem.UpdateSnapshot(productName, unitPrice);
            existingItem.IncreaseQuantity(quantity);
        }

        Touch();
    }

    public void UpdateItemQuantity(Guid productId, int quantity)
    {
        if (productId == Guid.Empty)
        {
            Trace.TraceError("Invalid product id while updating cart quantity.");
            return;
        }

        var item = Items.FirstOrDefault(existingItem => existingItem.ProductId == productId);

        if (item is null)
        {
            Trace.TraceError("Cart item {0} was not found while updating quantity.", productId);
            return;
        }

        if (quantity == 0)
        {
            Items.Remove(item);
        }
        else
        {
            item.SetQuantity(quantity);
        }

        Touch();
    }

    public void RemoveItem(Guid productId)
    {
        if (productId == Guid.Empty)
        {
            Trace.TraceError("Invalid product id while removing cart item.");
            return;
        }

        var item = Items.FirstOrDefault(existingItem => existingItem.ProductId == productId);

        if (item is null)
        {
            Trace.TraceError("Cart item {0} was not found while removing item.", productId);
            return;
        }

        Items.Remove(item);
        Touch();
    }

    public void Clear()
    {
        Items.Clear();
        Touch();
    }

    private void Touch() => UpdatedAtUtc = DateTime.UtcNow;

    private static void ValidateOwner(string ownerId, CartOwnerType ownerType)
    {
        if (string.IsNullOrWhiteSpace(ownerId))
            Trace.TraceError("Invalid owner id while creating cart.");

        if (!Enum.IsDefined(ownerType))
            Trace.TraceError("Invalid owner type while creating cart.");
    }
}
