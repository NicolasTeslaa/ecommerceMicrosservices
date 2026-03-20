using Order.Domain.Entities;
using Order.Domain.Exceptions;

namespace Order.Tests.Domain;

public class OrderItemTests
{
    [Fact]
    public void Constructor_ShouldCreateItem_WhenDataIsValid()
    {
        var item = new OrderItem(Guid.NewGuid(), "Produto", 100m, 2);

        Assert.NotEqual(Guid.Empty, item.Id);
        Assert.Equal("Produto", item.ProductName);
        Assert.Equal(100m, item.UnitPrice);
        Assert.Equal(2, item.Quantity);
        Assert.Equal(200m, item.TotalPrice);
    }

    [Fact]
    public void Constructor_ShouldTrimProductName_WhenNameHasWhitespace()
    {
        var item = new OrderItem(Guid.NewGuid(), " Produto ", 10m, 1);

        Assert.Equal("Produto", item.ProductName);
    }

    [Fact]
    public void Constructor_ShouldThrowInvalidProductIdException_WhenProductIdIsEmpty()
    {
        var act = () => new OrderItem(Guid.Empty, "Produto", 10m, 1);

        Assert.Throws<InvalidProductIdException>(act);
    }

    [Fact]
    public void Constructor_ShouldThrowInvalidProductNameException_WhenProductNameIsNull()
    {
        var act = () => new OrderItem(Guid.NewGuid(), null!, 10m, 1);

        Assert.Throws<InvalidProductNameException>(act);
    }

    [Fact]
    public void Constructor_ShouldThrowInvalidProductNameException_WhenProductNameIsWhitespace()
    {
        var act = () => new OrderItem(Guid.NewGuid(), "   ", 10m, 1);

        Assert.Throws<InvalidProductNameException>(act);
    }

    [Fact]
    public void Constructor_ShouldThrowInvalidUnitPriceException_WhenUnitPriceIsZero()
    {
        var act = () => new OrderItem(Guid.NewGuid(), "Produto", 0m, 1);

        Assert.Throws<InvalidUnitPriceException>(act);
    }

    [Fact]
    public void Constructor_ShouldThrowInvalidUnitPriceException_WhenUnitPriceIsNegative()
    {
        var act = () => new OrderItem(Guid.NewGuid(), "Produto", -1m, 1);

        Assert.Throws<InvalidUnitPriceException>(act);
    }

    [Fact]
    public void Constructor_ShouldThrowInvalidQuantityException_WhenQuantityIsZero()
    {
        var act = () => new OrderItem(Guid.NewGuid(), "Produto", 10m, 0);

        Assert.Throws<InvalidQuantityException>(act);
    }

    [Fact]
    public void Constructor_ShouldThrowInvalidQuantityException_WhenQuantityIsNegative()
    {
        var act = () => new OrderItem(Guid.NewGuid(), "Produto", 10m, -1);

        Assert.Throws<InvalidQuantityException>(act);
    }
}
