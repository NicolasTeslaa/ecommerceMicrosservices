using Cart.API.Controllers;
using Cart.Application.Commands;
using Cart.Application.DTOs;
using Cart.Application.Queries;
using Cart.Domain.Enums;
using ECommerce.Shared.Contracts;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Cart.Tests.Controllers;

public class CartControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly CartController _controller;

    public CartControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new CartController(_mediatorMock.Object);
    }

    [Fact]
    public async Task GetCart_ShouldReturnOk_WithApiResponse()
    {
        var cart = new CartDto
        {
            Id = Guid.NewGuid(),
            OwnerId = "guest-123",
            OwnerType = CartOwnerType.Guest
        };

        _mediatorMock
            .Setup(x => x.Send(
                It.Is<GetCartQuery>(query => query.OwnerId == "guest-123" && query.OwnerType == CartOwnerType.Guest),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(cart);

        var result = await _controller.GetCart(CartOwnerType.Guest, "guest-123");

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<CartDto>>(okResult.Value);

        Assert.True(response.Success);
        Assert.Equal(cart, response.Data);
    }

    [Fact]
    public async Task AddItem_ShouldSetOwnerFields_AndReturnOk()
    {
        var command = new AddCartItemCommand
        {
            ProductId = Guid.NewGuid(),
            ProductName = "GPU",
            UnitPrice = 3000m,
            Quantity = 1
        };

        var cart = new CartDto
        {
            Id = Guid.NewGuid(),
            OwnerId = "guest-123",
            OwnerType = CartOwnerType.Guest
        };

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<AddCartItemCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cart);

        var result = await _controller.AddItem(CartOwnerType.Guest, "guest-123", command);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<CartDto>>(okResult.Value);

        Assert.True(response.Success);
        Assert.Equal("guest-123", command.OwnerId);
        Assert.Equal(CartOwnerType.Guest, command.OwnerType);
        Assert.Equal(cart, response.Data);
    }

    [Fact]
    public async Task UpdateItemQuantity_ShouldSetRouteValuesIntoCommand()
    {
        var productId = Guid.NewGuid();
        var command = new UpdateCartItemQuantityCommand
        {
            Quantity = 2
        };

        var cart = new CartDto
        {
            Id = Guid.NewGuid(),
            OwnerId = "guest-123",
            OwnerType = CartOwnerType.Guest
        };

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<UpdateCartItemQuantityCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cart);

        var result = await _controller.UpdateItemQuantity(CartOwnerType.Guest, "guest-123", productId, command);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<CartDto>>(okResult.Value);

        Assert.True(response.Success);
        Assert.Equal("guest-123", command.OwnerId);
        Assert.Equal(CartOwnerType.Guest, command.OwnerType);
        Assert.Equal(productId, command.ProductId);
        Assert.Equal(cart, response.Data);
    }

    [Fact]
    public async Task RemoveItem_ShouldReturnOk_WithApiResponse()
    {
        var productId = Guid.NewGuid();
        var cart = new CartDto
        {
            Id = Guid.NewGuid(),
            OwnerId = "guest-123",
            OwnerType = CartOwnerType.Guest
        };

        _mediatorMock
            .Setup(x => x.Send(
                It.Is<RemoveCartItemCommand>(command =>
                    command.OwnerId == "guest-123" &&
                    command.OwnerType == CartOwnerType.Guest &&
                    command.ProductId == productId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(cart);

        var result = await _controller.RemoveItem(CartOwnerType.Guest, "guest-123", productId);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<CartDto>>(okResult.Value);

        Assert.True(response.Success);
        Assert.Equal(cart, response.Data);
    }

    [Fact]
    public async Task Clear_ShouldReturnOk_WithApiResponse()
    {
        var cart = new CartDto
        {
            Id = Guid.Empty,
            OwnerId = "guest-123",
            OwnerType = CartOwnerType.Guest
        };

        _mediatorMock
            .Setup(x => x.Send(
                It.Is<ClearCartCommand>(command =>
                    command.OwnerId == "guest-123" &&
                    command.OwnerType == CartOwnerType.Guest),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(cart);

        var result = await _controller.Clear(CartOwnerType.Guest, "guest-123");

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<CartDto>>(okResult.Value);

        Assert.True(response.Success);
        Assert.Equal(cart, response.Data);
    }
}
