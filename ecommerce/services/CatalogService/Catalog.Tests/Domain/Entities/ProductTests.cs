using Catalog.Domain.Entities;
using Catalog.Domain.Exceptions;

namespace Catalog.Tests.Domain.Entities;

public class ProductTests
{
    [Fact]
    public void Constructor_ShouldCreateProduct_WhenDataIsValid()
    {
        // Arrange
        var categoryId = Guid.NewGuid();

        // Act
        var product = new Product(
            " Notebook ",
            " Produto de teste ",
            3500m,
            10,
            categoryId);

        // Assert
        Assert.NotEqual(Guid.Empty, product.Id);
        Assert.Equal("Notebook", product.Name);
        Assert.Equal("Produto de teste", product.Description);
        Assert.Equal(3500m, product.Price);
        Assert.Equal(10, product.StockQuantity);
        Assert.Equal(categoryId, product.CategoryId);
        Assert.True(product.Active);
    }

    [Fact]
    public void Constructor_ShouldSetEmptyDescription_WhenDescriptionIsNull()
    {
        // Arrange
        var categoryId = Guid.NewGuid();

        // Act
        var product = new Product(
            "Produto",
            null!,
            100m,
            5,
            categoryId);

        // Assert
        Assert.Equal(string.Empty, product.Description);
    }

    [Fact]
    public void Constructor_ShouldThrowInvalidProductNameException_WhenNameIsEmpty()
    {
        // Arrange / Act
        var act = () => new Product(
            "",
            "Descrição",
            100m,
            5,
            Guid.NewGuid());

        // Assert
        Assert.Throws<InvalidProductNameException>(act);
    }

    [Fact]
    public void Constructor_ShouldThrowInvalidProductNameException_WhenNameIsWhiteSpace()
    {
        // Arrange / Act
        var act = () => new Product(
            "   ",
            "Descrição",
            100m,
            5,
            Guid.NewGuid());

        // Assert
        Assert.Throws<InvalidProductNameException>(act);
    }

    [Fact]
    public void Constructor_ShouldThrowInvalidProductPriceException_WhenPriceIsZero()
    {
        // Arrange / Act
        var act = () => new Product(
            "Produto",
            "Descrição",
            0m,
            5,
            Guid.NewGuid());

        // Assert
        Assert.Throws<InvalidProductPriceException>(act);
    }

    [Fact]
    public void Constructor_ShouldThrowInvalidProductPriceException_WhenPriceIsNegative()
    {
        // Arrange / Act
        var act = () => new Product(
            "Produto",
            "Descrição",
            -10m,
            5,
            Guid.NewGuid());

        // Assert
        Assert.Throws<InvalidProductPriceException>(act);
    }

    [Fact]
    public void Constructor_ShouldThrowInvalidStockQuantityException_WhenStockQuantityIsZero()
    {
        // Arrange / Act
        var act = () => new Product(
            "Produto",
            "Descrição",
            100m,
            0,
            Guid.NewGuid());

        // Assert
        Assert.Throws<InvalidStockQuantityException>(act);
    }

    [Fact]
    public void Constructor_ShouldThrowInvalidStockQuantityException_WhenStockQuantityIsNegative()
    {
        // Arrange / Act
        var act = () => new Product(
            "Produto",
            "Descrição",
            100m,
            -1,
            Guid.NewGuid());

        // Assert
        Assert.Throws<InvalidStockQuantityException>(act);
    }

    [Fact]
    public void Constructor_ShouldThrowInvalidCategoryIdException_WhenCategoryIdIsEmpty()
    {
        // Arrange / Act
        var act = () => new Product(
            "Produto",
            "Descrição",
            100m,
            5,
            Guid.Empty);

        // Assert
        Assert.Throws<InvalidCategoryIdException>(act);
    }

    [Fact]
    public void Update_ShouldUpdateProduct_WhenDataIsValid()
    {
        // Arrange
        var product = new Product(
            "Produto antigo",
            "Descrição antiga",
            100m,
            5,
            Guid.NewGuid());

        var newCategoryId = Guid.NewGuid();

        // Act
        product.Update(
            " Produto novo ",
            " Nova descrição ",
            250m,
            20,
            newCategoryId);

        // Assert
        Assert.Equal("Produto novo", product.Name);
        Assert.Equal("Nova descrição", product.Description);
        Assert.Equal(250m, product.Price);
        Assert.Equal(20, product.StockQuantity);
        Assert.Equal(newCategoryId, product.CategoryId);
        Assert.True(product.Active);
    }

    [Fact]
    public void Update_ShouldSetEmptyDescription_WhenDescriptionIsNull()
    {
        // Arrange
        var product = new Product(
            "Produto",
            "Descrição",
            100m,
            5,
            Guid.NewGuid());

        // Act
        product.Update(
            "Produto atualizado",
            null!,
            200m,
            10,
            Guid.NewGuid());

        // Assert
        Assert.Equal(string.Empty, product.Description);
    }

    [Fact]
    public void Update_ShouldThrowInvalidProductNameException_WhenNameIsInvalid()
    {
        // Arrange
        var product = new Product(
            "Produto",
            "Descrição",
            100m,
            5,
            Guid.NewGuid());

        // Act
        var act = () => product.Update(
            "",
            "Nova descrição",
            200m,
            10,
            Guid.NewGuid());

        // Assert
        Assert.Throws<InvalidProductNameException>(act);
    }

    [Fact]
    public void Update_ShouldThrowInvalidProductPriceException_WhenPriceIsInvalid()
    {
        // Arrange
        var product = new Product(
            "Produto",
            "Descrição",
            100m,
            5,
            Guid.NewGuid());

        // Act
        var act = () => product.Update(
            "Produto atualizado",
            "Nova descrição",
            0m,
            10,
            Guid.NewGuid());

        // Assert
        Assert.Throws<InvalidProductPriceException>(act);
    }

    [Fact]
    public void Update_ShouldThrowInvalidStockQuantityException_WhenStockQuantityIsInvalid()
    {
        // Arrange
        var product = new Product(
            "Produto",
            "Descrição",
            100m,
            5,
            Guid.NewGuid());

        // Act
        var act = () => product.Update(
            "Produto atualizado",
            "Nova descrição",
            200m,
            0,
            Guid.NewGuid());

        // Assert
        Assert.Throws<InvalidStockQuantityException>(act);
    }

    [Fact]
    public void Update_ShouldThrowInvalidCategoryIdException_WhenCategoryIdIsInvalid()
    {
        // Arrange
        var product = new Product(
            "Produto",
            "Descrição",
            100m,
            5,
            Guid.NewGuid());

        // Act
        var act = () => product.Update(
            "Produto atualizado",
            "Nova descrição",
            200m,
            10,
            Guid.Empty);

        // Assert
        Assert.Throws<InvalidCategoryIdException>(act);
    }

    [Fact]
    public void Deactivate_ShouldSetActiveToFalse()
    {
        // Arrange
        var product = new Product(
            "Produto",
            "Descrição",
            100m,
            5,
            Guid.NewGuid());

        // Act
        product.Deactivate();

        // Assert
        Assert.False(product.Active);
    }

    [Fact]
    public void Activate_ShouldSetActiveToTrue()
    {
        // Arrange
        var product = new Product(
            "Produto",
            "Descrição",
            100m,
            5,
            Guid.NewGuid());

        product.Deactivate();

        // Act
        product.Activate();

        // Assert
        Assert.True(product.Active);
    }
}