using Cart.Application.Commands;
using Cart.Application.DTOs;
using Cart.Application.Interfaces;
using MediatR;

namespace Cart.Application.Handlers;

public class AddCartItemHandler : IRequestHandler<AddCartItemCommand, CartDto>
{
    private readonly ICartRepository _repository;

    public AddCartItemHandler(ICartRepository repository) => _repository = repository;

    public async Task<CartDto> Handle(AddCartItemCommand request, CancellationToken cancellationToken)
    {
        var ownerId = request.OwnerId?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(ownerId))
            return CartDto.Empty(string.Empty, request.OwnerType);

        if (!Enum.IsDefined(request.OwnerType))
            return CartDto.Empty(ownerId, request.OwnerType);

        var cart = await _repository.GetByOwnerAsync(ownerId, request.OwnerType, cancellationToken);
        var isNewCart = cart is null;

        cart ??= new Cart.Domain.Entities.Cart(ownerId, request.OwnerType);

        cart.AddItem(request.ProductId, request.ProductName, request.UnitPrice, request.Quantity);

        if (isNewCart)
            await _repository.AddAsync(cart, cancellationToken);
        else
            await _repository.UpdateAsync(cart, cancellationToken);

        return CartDto.MapFromEntity(cart);
    }
}
