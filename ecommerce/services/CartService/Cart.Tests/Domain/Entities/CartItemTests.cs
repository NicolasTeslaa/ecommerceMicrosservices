using Cart.Domain.Entities;
using Cart.Domain.Exceptions;

namespace Cart.Tests.Domain.Entities;

public class CartItemTests
{
    [Fact]
    public void Constructor_ShouldCreateCartItem_WhenDataIsValid()
    {
        var productId = Guid.NewGuid();

        var item = new CartItem(productId, "Notebook", 3500m, 2);

        Assert.NotEqual(Guid.Empty, item.Id);
        Assert.Equal(productId, item.ProductId);
        Assert.Equal("Notebook", item.ProductName);
        Assert.Equal(3500m, item.UnitPrice);
        Assert.Equal(2, item.Quantity);
        Assert.Equal(7000m, item.Subtotal);
    }

    [Fact]
    public void Constructor_ShouldThrowInvalidProductNameException_WhenNameIsInvalid()
    {
        var act = () => new CartItem(Guid.NewGuid(), " ", 3500m, 1);

        Assert.Throws<InvalidProductNameException>(act);
    }

    [Fact]
    public void Constructor_ShouldThrowInvalidUnitPriceException_WhenPriceIsInvalid()
    {
        var act = () => new CartItem(Guid.NewGuid(), "Notebook", 0m, 1);

        Assert.Throws<InvalidUnitPriceException>(act);
    }

    [Fact]
    public void Constructor_ShouldThrowInvalidQuantityException_WhenQuantityIsInvalid()
    {
        var act = () => new CartItem(Guid.NewGuid(), "Notebook", 3500m, 0);

        Assert.Throws<InvalidQuantityException>(act);
    }

    [Fact]
    public void IncreaseQuantity_ShouldIncreaseQuantity()
    {
        var item = new CartItem(Guid.NewGuid(), "Notebook", 3500m, 1);

        item.IncreaseQuantity(2);

        Assert.Equal(3, item.Quantity);
        Assert.Equal(10500m, item.Subtotal);
    }

    [Fact]
    public void UpdateSnapshot_ShouldUpdateNameAndPrice()
    {
        var item = new CartItem(Guid.NewGuid(), "Notebook", 3500m, 1);

        item.UpdateSnapshot("Notebook Pro", 4000m);

        Assert.Equal("Notebook Pro", item.ProductName);
        Assert.Equal(4000m, item.UnitPrice);
        Assert.Equal(4000m, item.Subtotal);
    }
}
