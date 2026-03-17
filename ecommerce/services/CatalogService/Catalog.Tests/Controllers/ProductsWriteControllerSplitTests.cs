using Catalog.API.Write.Controllers;
using Catalog.Application.Commands;
using ECommerce.Shared.Contracts;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Catalog.Tests.Controllers;

public class ProductsWriteControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly ProductsWriteController _controller;

    public ProductsWriteControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new ProductsWriteController(_mediatorMock.Object);
    }

    [Fact]
    public async Task Create_ShouldReturnAccepted_WithApiResponse()
    {
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

        var result = await _controller.Create(command);

        var acceptedResult = Assert.IsType<AcceptedResult>(result.Result);
        var response = Assert.IsType<ApiResponse<Guid>>(acceptedResult.Value);
        Assert.True(response.Success);
        Assert.Equal("Product created successfully.", response.Message);
        Assert.Equal(productId, response.Data);

        _mediatorMock.Verify(x => x.Send(command, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_ShouldSetCommandId_AndReturnOk_WithApiResponse()
    {
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

        var result = await _controller.Update(productId, command);

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
        var productId = Guid.NewGuid();

        _mediatorMock
            .Setup(x => x.Send(It.Is<DeactivateProductCommand>(c => c.Id == productId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(productId);

        var result = await _controller.Delete(productId);

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
