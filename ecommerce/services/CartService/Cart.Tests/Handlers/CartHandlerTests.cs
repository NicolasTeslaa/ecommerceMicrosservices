using Cart.Application.Commands;
using Cart.Application.Handlers;
using Cart.Application.Interfaces;
using Cart.Application.Queries;
using Cart.Domain.Entities;
using Cart.Domain.Enums;
using Cart.Domain.Exceptions;
using Moq;

namespace Cart.Tests.Handlers;

public class GetCartHandlerTests
{
    private readonly Mock<ICartRepository> _repositoryMock;
    private readonly GetCartHandler _handler;

    public GetCartHandlerTests()
    {
        _repositoryMock = new Mock<ICartRepository>();
        _handler = new GetCartHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyCartDto_WhenCartDoesNotExist()
    {
        var query = new GetCartQuery("guest-123", CartOwnerType.Guest);

        _repositoryMock
            .Setup(x => x.GetByOwnerAsync("guest-123", CartOwnerType.Guest, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cart.Domain.Entities.Cart?)null);

        var result = await _handler.Handle(query, CancellationToken.None);

        Assert.Equal(Guid.Empty, result.Id);
        Assert.Equal("guest-123", result.OwnerId);
        Assert.Empty(result.Items);
    }
}

public class AddCartItemHandlerTests
{
    private readonly Mock<ICartRepository> _repositoryMock;
    private readonly AddCartItemHandler _handler;

    public AddCartItemHandlerTests()
    {
        _repositoryMock = new Mock<ICartRepository>();
        _handler = new AddCartItemHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateCart_AndCallAddAsync_WhenCartDoesNotExist()
    {
        Cart.Domain.Entities.Cart? capturedCart = null;
        var command = new AddCartItemCommand
        {
            OwnerId = "guest-123",
            OwnerType = CartOwnerType.Guest,
            ProductId = Guid.NewGuid(),
            ProductName = "GPU",
            UnitPrice = 3000m,
            Quantity = 2
        };

        _repositoryMock
            .Setup(x => x.GetByOwnerAsync(command.OwnerId, command.OwnerType, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cart.Domain.Entities.Cart?)null);

        _repositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Cart.Domain.Entities.Cart>(), It.IsAny<CancellationToken>()))
            .Callback<Cart.Domain.Entities.Cart, CancellationToken>((cart, _) => capturedCart = cart)
            .Returns(Task.CompletedTask);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.NotNull(capturedCart);
        Assert.Equal(capturedCart!.Id, result.Id);
        Assert.Single(result.Items);

        _repositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Cart.Domain.Entities.Cart>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _repositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<Cart.Domain.Entities.Cart>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldUpdateCart_WhenCartAlreadyExists()
    {
        var existingCart = new Cart.Domain.Entities.Cart("guest-123", CartOwnerType.Guest);
        existingCart.AddItem(Guid.NewGuid(), "CPU", 1000m, 1);

        var command = new AddCartItemCommand
        {
            OwnerId = "guest-123",
            OwnerType = CartOwnerType.Guest,
            ProductId = Guid.NewGuid(),
            ProductName = "GPU",
            UnitPrice = 3000m,
            Quantity = 1
        };

        _repositoryMock
            .Setup(x => x.GetByOwnerAsync(command.OwnerId, command.OwnerType, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCart);

        _repositoryMock
            .Setup(x => x.UpdateAsync(existingCart, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.Equal(2, result.Items.Count);

        _repositoryMock.Verify(
            x => x.UpdateAsync(existingCart, It.IsAny<CancellationToken>()),
            Times.Once);

        _repositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Cart.Domain.Entities.Cart>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}

public class UpdateCartItemQuantityHandlerTests
{
    private readonly Mock<ICartRepository> _repositoryMock;
    private readonly UpdateCartItemQuantityHandler _handler;

    public UpdateCartItemQuantityHandlerTests()
    {
        _repositoryMock = new Mock<ICartRepository>();
        _handler = new UpdateCartItemQuantityHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowCartNotFoundException_WhenCartDoesNotExist()
    {
        var command = new UpdateCartItemQuantityCommand
        {
            OwnerId = "guest-123",
            OwnerType = CartOwnerType.Guest,
            ProductId = Guid.NewGuid(),
            Quantity = 2
        };

        _repositoryMock
            .Setup(x => x.GetByOwnerAsync(command.OwnerId, command.OwnerType, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cart.Domain.Entities.Cart?)null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await Assert.ThrowsAsync<CartNotFoundException>(act);
    }
}

public class RemoveCartItemHandlerTests
{
    private readonly Mock<ICartRepository> _repositoryMock;
    private readonly RemoveCartItemHandler _handler;

    public RemoveCartItemHandlerTests()
    {
        _repositoryMock = new Mock<ICartRepository>();
        _handler = new RemoveCartItemHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldRemoveItem_AndUpdateCart()
    {
        var productId = Guid.NewGuid();
        var cart = new Cart.Domain.Entities.Cart("guest-123", CartOwnerType.Guest);
        cart.AddItem(productId, "CPU", 1000m, 1);

        var command = new RemoveCartItemCommand
        {
            OwnerId = "guest-123",
            OwnerType = CartOwnerType.Guest,
            ProductId = productId
        };

        _repositoryMock
            .Setup(x => x.GetByOwnerAsync(command.OwnerId, command.OwnerType, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cart);

        _repositoryMock
            .Setup(x => x.UpdateAsync(cart, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.Empty(result.Items);
        _repositoryMock.Verify(x => x.UpdateAsync(cart, It.IsAny<CancellationToken>()), Times.Once);
    }
}

public class ClearCartHandlerTests
{
    private readonly Mock<ICartRepository> _repositoryMock;
    private readonly ClearCartHandler _handler;

    public ClearCartHandlerTests()
    {
        _repositoryMock = new Mock<ICartRepository>();
        _handler = new ClearCartHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyCart_WhenCartDoesNotExist()
    {
        var command = new ClearCartCommand
        {
            OwnerId = "guest-123",
            OwnerType = CartOwnerType.Guest
        };

        _repositoryMock
            .Setup(x => x.GetByOwnerAsync(command.OwnerId, command.OwnerType, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cart.Domain.Entities.Cart?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.Equal(Guid.Empty, result.Id);
        Assert.Empty(result.Items);

        _repositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<Cart.Domain.Entities.Cart>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
