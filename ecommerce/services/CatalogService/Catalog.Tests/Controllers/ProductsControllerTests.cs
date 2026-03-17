using Catalog.API.Controllers;
using Catalog.API.Responses;
using Catalog.Application.Commands;
using Catalog.Application.DTOs;
using Catalog.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Catalog.Tests.Controllers;

public class ProductsControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly ProductsController _controller;

    public ProductsControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new ProductsController(_mediatorMock.Object);
    }

    [Fact]
    public async Task Create_ShouldReturnCreatedAtAction_WithApiResponse()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var command = new CreateProductCommand
        {
            Name = "Notebook",
            Description = "Notebook gamer",
            Price = 4500m,
            StockQuantity = 10,
            CategoryId = Guid.NewGuid()
        };

        _mediatorMock
            .Setup(x => x.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(productId);

        // Act
        var result = await _controller.Create(command);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(ProductsController.GetById), createdAtActionResult.ActionName);

        var response = Assert.IsType<ApiResponse<Guid>>(createdAtActionResult.Value);
        Assert.True(response.Success);
        Assert.Equal("Product created successfully.", response.Message);
        Assert.Equal(productId, response.Data);

        _mediatorMock.Verify(x => x.Send(command, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WithApiResponse()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var productDto = new ProductDto
        {
            Id = productId,
            Name = "Mouse",
            Description = "Mouse sem fio",
            Price = 120m,
            StockQuantity = 30,
            Active = true,
            CategoryId = Guid.NewGuid()
        };

        _mediatorMock
            .Setup(x => x.Send(It.Is<GetProductByIdQuery>(q => q.Id == productId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(productDto);

        // Act
        var result = await _controller.GetById(productId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<ProductDto>>(okResult.Value);

        Assert.True(response.Success);
        Assert.Equal("Product retrieved successfully.", response.Message);
        Assert.NotNull(response.Data);
        Assert.Equal(productId, response.Data!.Id);
        Assert.Equal("Mouse", response.Data.Name);

        _mediatorMock.Verify(
            x => x.Send(It.Is<GetProductByIdQuery>(q => q.Id == productId), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAll_ShouldReturnOk_WithApiResponse()
    {
        // Arrange
        var products = new List<ProductDto>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Produto 1",
                Description = "Desc 1",
                Price = 100m,
                StockQuantity = 5,
                Active = true,
                CategoryId = Guid.NewGuid()
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Produto 2",
                Description = "Desc 2",
                Price = 200m,
                StockQuantity = 8,
                Active = true,
                CategoryId = Guid.NewGuid()
            }
        };

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetAllProductsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<IEnumerable<ProductDto>>>(okResult.Value);

        Assert.True(response.Success);
        Assert.Equal("Products retrieved successfully.", response.Message);
        Assert.NotNull(response.Data);
        Assert.Equal(2, response.Data!.Count());

        _mediatorMock.Verify(
            x => x.Send(It.IsAny<GetAllProductsQuery>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Update_ShouldSetCommandId_AndReturnOk_WithApiResponse()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var command = new UpdateProductCommand
        {
            Name = "Teclado",
            Description = "Teclado mecânico",
            Price = 350m,
            StockQuantity = 12,
            CategoryId = Guid.NewGuid()
        };

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<UpdateProductCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(productId);

        // Act
        var result = await _controller.Update(productId, command);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<Guid>>(okResult.Value);

        Assert.True(response.Success);
        Assert.Equal("Product updated successfully.", response.Message);
        Assert.Equal(productId, response.Data);
        Assert.Equal(productId, command.Id);

        _mediatorMock.Verify(
            x => x.Send(It.Is<UpdateProductCommand>(c => c.Id == productId), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Delete_ShouldReturnOk_WithApiResponse()
    {
        // Arrange
        var productId = Guid.NewGuid();

        _mediatorMock
            .Setup(x => x.Send(It.Is<DeactivateProductCommand>(c => c.Id == productId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(productId);

        // Act
        var result = await _controller.Delete(productId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<Guid>>(okResult.Value);

        Assert.True(response.Success);
        Assert.Equal("Product deleted successfully.", response.Message);
        Assert.Equal(productId, response.Data);

        _mediatorMock.Verify(
            x => x.Send(It.Is<DeactivateProductCommand>(c => c.Id == productId), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}