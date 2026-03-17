using Catalog.Application.Commands;
using Catalog.Application.Handlers;
using Catalog.Application.Interfaces;
using Catalog.Application.Queries;
using Catalog.Domain.Entities;
using Catalog.Domain.Exceptions;
using Moq;

namespace Catalog.Tests.Handlers;

public class CreateProductHandlerTests
{
    private readonly Mock<IProductRepository> _repositoryMock;
    private readonly CreateProductHandler _handler;

    public CreateProductHandlerTests()
    {
        _repositoryMock = new Mock<IProductRepository>();
        _handler = new CreateProductHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateProduct_AndReturnId()
    {
        // Arrange
        Product? capturedProduct = null;

        var command = new CreateProductCommand
        {
            Name = "Notebook",
            Description = "Notebook gamer",
            Price = 5000m,
            StockQuantity = 10,
            CategoryId = Guid.NewGuid()
        };

        _repositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Callback<Product, CancellationToken>((product, _) => capturedProduct = product)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, result);
        Assert.NotNull(capturedProduct);
        Assert.Equal(command.Name, capturedProduct!.Name);
        Assert.Equal(command.Description, capturedProduct.Description);
        Assert.Equal(command.Price, capturedProduct.Price);
        Assert.Equal(command.StockQuantity, capturedProduct.StockQuantity);
        Assert.Equal(command.CategoryId, capturedProduct.CategoryId);
        Assert.Equal(capturedProduct.Id, result);

        _repositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}

public class DeactivateProductHandlerTests
{
    private readonly Mock<IProductRepository> _repositoryMock;
    private readonly DeactivateProductHandler _handler;

    public DeactivateProductHandlerTests()
    {
        _repositoryMock = new Mock<IProductRepository>();
        _handler = new DeactivateProductHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidProductIdException_WhenIdIsEmpty()
    {
        // Arrange
        var command = new DeactivateProductCommand(Guid.Empty);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<InvalidProductIdException>(act);

        _repositoryMock.Verify(
            x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _repositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowProductNotFoundException_WhenProductDoesNotExist()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var command = new DeactivateProductCommand(productId);

        _repositoryMock
            .Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ProductNotFoundException>(act);

        _repositoryMock.Verify(
            x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()),
            Times.Once);

        _repositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldDeactivateProduct_AndReturnId()
    {
        // Arrange
        var product = new Product(
            "Mouse",
            "Mouse gamer",
            150m,
            20,
            Guid.NewGuid());

        var command = new DeactivateProductCommand(product.Id);

        _repositoryMock
            .Setup(x => x.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _repositoryMock
            .Setup(x => x.UpdateAsync(product, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(product.Id, result);
        Assert.False(product.Active);

        _repositoryMock.Verify(
            x => x.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()),
            Times.Once);

        _repositoryMock.Verify(
            x => x.UpdateAsync(product, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}

public class GetAllProductsHandlerTests
{
    private readonly Mock<IProductRepository> _repositoryMock;
    private readonly GetAllProductsHandler _handler;

    public GetAllProductsHandlerTests()
    {
        _repositoryMock = new Mock<IProductRepository>();
        _handler = new GetAllProductsHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnMappedProducts()
    {
        // Arrange
        var products = new List<Product>
        {
            new("Produto 1", "Desc 1", 100m, 5, Guid.NewGuid()),
            new("Produto 2", "Desc 2", 200m, 8, Guid.NewGuid())
        };

        _repositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        // Act
        var result = await _handler.Handle(new GetAllProductsQuery(), CancellationToken.None);

        // Assert
        var list = result.ToList();

        Assert.Equal(2, list.Count);
        Assert.Equal(products[0].Id, list[0].Id);
        Assert.Equal(products[0].Name, list[0].Name);
        Assert.Equal(products[1].Id, list[1].Id);
        Assert.Equal(products[1].Name, list[1].Name);

        _repositoryMock.Verify(
            x => x.GetAllAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenRepositoryReturnsEmpty()
    {
        // Arrange
        _repositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<Product>());

        // Act
        var result = await _handler.Handle(new GetAllProductsQuery(), CancellationToken.None);

        // Assert
        Assert.Empty(result);

        _repositoryMock.Verify(
            x => x.GetAllAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }
}

public class GetProductByIdHandlerTests
{
    private readonly Mock<IProductRepository> _repositoryMock;
    private readonly GetProductByIdHandler _handler;

    public GetProductByIdHandlerTests()
    {
        _repositoryMock = new Mock<IProductRepository>();
        _handler = new GetProductByIdHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidProductIdException_WhenIdIsEmpty()
    {
        // Arrange
        var query = new GetProductByIdQuery(Guid.Empty);

        // Act
        var act = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<InvalidProductIdException>(act);

        _repositoryMock.Verify(
            x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowProductNotFoundException_WhenProductDoesNotExist()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var query = new GetProductByIdQuery(productId);

        _repositoryMock
            .Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        var act = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ProductNotFoundException>(act);

        _repositoryMock.Verify(
            x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnMappedProductDto_WhenProductExists()
    {
        // Arrange
        var product = new Product(
            "Teclado",
            "Teclado mecânico",
            300m,
            12,
            Guid.NewGuid());

        var query = new GetProductByIdQuery(product.Id);

        _repositoryMock
            .Setup(x => x.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(product.Id, result!.Id);
        Assert.Equal(product.Name, result.Name);
        Assert.Equal(product.Description, result.Description);
        Assert.Equal(product.Price, result.Price);
        Assert.Equal(product.StockQuantity, result.StockQuantity);
        Assert.Equal(product.Active, result.Active);
        Assert.Equal(product.CategoryId, result.CategoryId);

        _repositoryMock.Verify(
            x => x.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}

public class UpdateProductHandlerTests
{
    private readonly Mock<IProductRepository> _repositoryMock;
    private readonly UpdateProductHandler _handler;

    public UpdateProductHandlerTests()
    {
        _repositoryMock = new Mock<IProductRepository>();
        _handler = new UpdateProductHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidProductIdException_WhenIdIsEmpty()
    {
        // Arrange
        var command = new UpdateProductCommand
        {
            Id = Guid.Empty,
            Name = "Novo nome",
            Description = "Nova descrição",
            Price = 999m,
            StockQuantity = 15,
            CategoryId = Guid.NewGuid()
        };

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<InvalidProductIdException>(act);

        _repositoryMock.Verify(
            x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _repositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowProductNotFoundException_WhenProductDoesNotExist()
    {
        // Arrange
        var productId = Guid.NewGuid();

        var command = new UpdateProductCommand
        {
            Id = productId,
            Name = "Novo nome",
            Description = "Nova descrição",
            Price = 999m,
            StockQuantity = 15,
            CategoryId = Guid.NewGuid()
        };

        _repositoryMock
            .Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ProductNotFoundException>(act);

        _repositoryMock.Verify(
            x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()),
            Times.Once);

        _repositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldUpdateProduct_AndReturnId()
    {
        // Arrange
        var product = new Product(
            "Nome antigo",
            "Descrição antiga",
            100m,
            5,
            Guid.NewGuid());

        var newCategoryId = Guid.NewGuid();

        var command = new UpdateProductCommand
        {
            Id = product.Id,
            Name = "Nome novo",
            Description = "Descrição nova",
            Price = 250m,
            StockQuantity = 20,
            CategoryId = newCategoryId
        };

        _repositoryMock
            .Setup(x => x.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _repositoryMock
            .Setup(x => x.UpdateAsync(product, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(product.Id, result);
        Assert.Equal(command.Name, product.Name);
        Assert.Equal(command.Description, product.Description);
        Assert.Equal(command.Price, product.Price);
        Assert.Equal(command.StockQuantity, product.StockQuantity);
        Assert.Equal(command.CategoryId, product.CategoryId);

        _repositoryMock.Verify(
            x => x.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()),
            Times.Once);

        _repositoryMock.Verify(
            x => x.UpdateAsync(product, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}