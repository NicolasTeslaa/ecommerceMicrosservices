using Catalog.Domain.Entities;
using Catalog.Domain.Exceptions;

namespace Catalog.Tests.Domain.Entities;

public class ProductTests
{
    private const decimal HeightCm = 10m;
    private const decimal WidthCm = 20m;
    private const decimal CubageM3 = 0.0100m;
    private const decimal WeightKg = 1.250m;
    private const string OriginZipCode = "01001-000";

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
            categoryId,
            HeightCm,
            WidthCm,
            CubageM3,
            WeightKg,
            OriginZipCode);

        // Assert
        Assert.NotEqual(Guid.Empty, product.Id);
        Assert.Equal("Notebook", product.Name);
        Assert.Equal("Produto de teste", product.Description);
        Assert.Equal(3500m, product.Price);
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
            categoryId,
            HeightCm,
            WidthCm,
            CubageM3,
            WeightKg,
            OriginZipCode);

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
            Guid.NewGuid(),
            HeightCm,
            WidthCm,
            CubageM3,
            WeightKg,
            OriginZipCode);

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
            Guid.NewGuid(),
            HeightCm,
            WidthCm,
            CubageM3,
            WeightKg,
            OriginZipCode);

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
            Guid.NewGuid(),
            HeightCm,
            WidthCm,
            CubageM3,
            WeightKg,
            OriginZipCode);

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
            Guid.NewGuid(),
            HeightCm,
            WidthCm,
            CubageM3,
            WeightKg,
            OriginZipCode);

        // Assert
        Assert.Throws<InvalidProductPriceException>(act);
    }

    [Fact]
    public void Constructor_ShouldThrowInvalidCategoryIdException_WhenCategoryIdIsEmpty()
    {
        // Arrange / Act
        var act = () => new Product(
            "Produto",
            "Descrição",
            100m,
            Guid.Empty,
            HeightCm,
            WidthCm,
            CubageM3,
            WeightKg,
            OriginZipCode);

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
            Guid.NewGuid(),
            HeightCm,
            WidthCm,
            CubageM3,
            WeightKg,
            OriginZipCode);

        var newCategoryId = Guid.NewGuid();

        // Act
        product.Update(
            " Produto novo ",
            " Nova descrição ",
            250m,
            newCategoryId,
            15m,
            25m,
            0.0150m,
            1.750m,
            "20040-002");

        // Assert
        Assert.Equal("Produto novo", product.Name);
        Assert.Equal("Nova descrição", product.Description);
        Assert.Equal(250m, product.Price);
        Assert.Equal(newCategoryId, product.CategoryId);
        Assert.Equal(15m, product.HeightCm);
        Assert.Equal(25m, product.WidthCm);
        Assert.Equal(0.0150m, product.CubageM3);
        Assert.Equal(1.750m, product.WeightKg);
        Assert.Equal("20040-002", product.OriginZipCode);
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
            Guid.NewGuid(),
            HeightCm,
            WidthCm,
            CubageM3,
            WeightKg,
            OriginZipCode);

        // Act
        product.Update(
            "Produto atualizado",
            null!,
            200m,
            Guid.NewGuid(),
            HeightCm,
            WidthCm,
            CubageM3,
            WeightKg,
            OriginZipCode);

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
            Guid.NewGuid(),
            HeightCm,
            WidthCm,
            CubageM3,
            WeightKg,
            OriginZipCode);

        // Act
        var act = () => product.Update(
            "",
            "Nova descrição",
            200m,
            Guid.NewGuid(),
            HeightCm,
            WidthCm,
            CubageM3,
            WeightKg,
            OriginZipCode);

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
            Guid.NewGuid(),
            HeightCm,
            WidthCm,
            CubageM3,
            WeightKg,
            OriginZipCode);

        // Act
        var act = () => product.Update(
            "Produto atualizado",
            "Nova descrição",
            0m,
            Guid.NewGuid(),
            HeightCm,
            WidthCm,
            CubageM3,
            WeightKg,
            OriginZipCode);

        // Assert
        Assert.Throws<InvalidProductPriceException>(act);
    }

    [Fact]
    public void Update_ShouldThrowInvalidCategoryIdException_WhenCategoryIdIsInvalid()
    {
        // Arrange
        var product = new Product(
            "Produto",
            "Descrição",
            100m,
            Guid.NewGuid(),
            HeightCm,
            WidthCm,
            CubageM3,
            WeightKg,
            OriginZipCode);

        // Act
        var act = () => product.Update(
            "Produto atualizado",
            "Nova descrição",
            200m,
            Guid.Empty,
            HeightCm,
            WidthCm,
            CubageM3,
            WeightKg,
            OriginZipCode);

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
            Guid.NewGuid(),
            HeightCm,
            WidthCm,
            CubageM3,
            WeightKg,
            OriginZipCode);

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
            Guid.NewGuid(),
            HeightCm,
            WidthCm,
            CubageM3,
            WeightKg,
            OriginZipCode);

        product.Deactivate();

        // Act
        product.Activate();

        // Assert
        Assert.True(product.Active);
    }
}
