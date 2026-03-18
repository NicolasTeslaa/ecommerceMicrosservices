using Cart.Application.Commands;
using Cart.Application.DTOs;
using Cart.Application.Interfaces;
using Cart.Domain.Exceptions;
using MediatR;

namespace Cart.Application.Handlers;

public class RemoveCartItemHandler : IRequestHandler<RemoveCartItemCommand, CartDto>
{
    private readonly ICartRepository _repository;

    public RemoveCartItemHandler(ICartRepository repository) => _repository = repository;

    public async Task<CartDto> Handle(RemoveCartItemCommand request, CancellationToken cancellationToken)
    {
        var ownerId = request.OwnerId?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(ownerId))
            throw new InvalidOwnerIdException();

        if (!Enum.IsDefined(request.OwnerType))
            throw new InvalidOwnerTypeException();

        var cart = await _repository.GetByOwnerAsync(ownerId, request.OwnerType, cancellationToken)
            ?? throw new CartNotFoundException(ownerId, request.OwnerType);

        cart.RemoveItem(request.ProductId);
        await _repository.UpdateAsync(cart, cancellationToken);

        return CartDto.MapFromEntity(cart);
    }
}
