using Cart.Domain.Enums;
using Cart.Domain.Exceptions;

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
        OwnerId = ownerId.Trim();
        OwnerType = ownerType;
        Status = CartStatus.Active;
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = CreatedAtUtc;
    }

    public void AddItem(Guid productId, string productName, decimal unitPrice, int quantity)
    {
        if (quantity <= 0)
            throw new InvalidQuantityException();

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
            throw new InvalidProductIdException();

        var item = Items.FirstOrDefault(existingItem => existingItem.ProductId == productId);

        if (item is null)
            throw new CartItemNotFoundException(productId);

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
            throw new InvalidProductIdException();

        var item = Items.FirstOrDefault(existingItem => existingItem.ProductId == productId);

        if (item is null)
            throw new CartItemNotFoundException(productId);

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
            throw new InvalidOwnerIdException();

        if (!Enum.IsDefined(ownerType))
            throw new InvalidOwnerTypeException();
    }
}
