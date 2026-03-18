using Cart.Domain.Entities;
using Cart.Domain.Enums;
using Cart.Domain.Exceptions;

namespace Cart.Tests.Domain.Entities;

public class CartTests
{
    [Fact]
    public void Constructor_ShouldCreateCart_WhenOwnerIsValid()
    {
        var cart = new Cart.Domain.Entities.Cart(" guest-123 ", CartOwnerType.Guest);

        Assert.NotEqual(Guid.Empty, cart.Id);
        Assert.Equal("guest-123", cart.OwnerId);
        Assert.Equal(CartOwnerType.Guest, cart.OwnerType);
        Assert.Equal(CartStatus.Active, cart.Status);
        Assert.Empty(cart.Items);
    }

    [Fact]
    public void Constructor_ShouldThrowInvalidOwnerIdException_WhenOwnerIdIsEmpty()
    {
        var act = () => new Cart.Domain.Entities.Cart("", CartOwnerType.Guest);

        Assert.Throws<InvalidOwnerIdException>(act);
    }

    [Fact]
    public void AddItem_ShouldAddNewItem_WhenProductDoesNotExistInCart()
    {
        var cart = new Cart.Domain.Entities.Cart("guest-123", CartOwnerType.Guest);
        var productId = Guid.NewGuid();

        cart.AddItem(productId, "GPU", 2500m, 2);

        var item = Assert.Single(cart.Items);
        Assert.Equal(productId, item.ProductId);
        Assert.Equal("GPU", item.ProductName);
        Assert.Equal(2500m, item.UnitPrice);
        Assert.Equal(2, item.Quantity);
        Assert.Equal(5000m, cart.TotalAmount);
    }

    [Fact]
    public void AddItem_ShouldIncreaseQuantity_WhenProductAlreadyExists()
    {
        var cart = new Cart.Domain.Entities.Cart("guest-123", CartOwnerType.Guest);
        var productId = Guid.NewGuid();

        cart.AddItem(productId, "GPU", 2500m, 1);
        cart.AddItem(productId, "GPU OC", 2600m, 2);

        var item = Assert.Single(cart.Items);
        Assert.Equal("GPU OC", item.ProductName);
        Assert.Equal(2600m, item.UnitPrice);
        Assert.Equal(3, item.Quantity);
        Assert.Equal(7800m, cart.TotalAmount);
    }

    [Fact]
    public void UpdateItemQuantity_ShouldRemoveItem_WhenQuantityIsZero()
    {
        var cart = new Cart.Domain.Entities.Cart("guest-123", CartOwnerType.Guest);
        var productId = Guid.NewGuid();

        cart.AddItem(productId, "CPU", 1000m, 1);
        cart.UpdateItemQuantity(productId, 0);

        Assert.Empty(cart.Items);
        Assert.Equal(0m, cart.TotalAmount);
    }

    [Fact]
    public void RemoveItem_ShouldThrowCartItemNotFoundException_WhenProductDoesNotExist()
    {
        var cart = new Cart.Domain.Entities.Cart("guest-123", CartOwnerType.Guest);

        var act = () => cart.RemoveItem(Guid.NewGuid());

        Assert.Throws<CartItemNotFoundException>(act);
    }

    [Fact]
    public void Clear_ShouldRemoveAllItems()
    {
        var cart = new Cart.Domain.Entities.Cart("guest-123", CartOwnerType.Guest);

        cart.AddItem(Guid.NewGuid(), "CPU", 1000m, 1);
        cart.AddItem(Guid.NewGuid(), "GPU", 2000m, 2);

        cart.Clear();

        Assert.Empty(cart.Items);
        Assert.Equal(0m, cart.TotalAmount);
    }
}
