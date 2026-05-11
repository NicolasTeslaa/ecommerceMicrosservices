using Cart.Application.DTOs;
using Cart.Application.Interfaces;
using Cart.Application.Queries;
using MediatR;

namespace Cart.Application.Handlers;

public class GetCartHandler : IRequestHandler<GetCartQuery, CartDto>
{
    private readonly ICartRepository _repository;

    public GetCartHandler(ICartRepository repository) => _repository = repository;

    public async Task<CartDto> Handle(GetCartQuery request, CancellationToken cancellationToken)
    {
        var ownerId = request.OwnerId?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(ownerId))
            return CartDto.Empty(string.Empty, request.OwnerType);

        if (!Enum.IsDefined(request.OwnerType))
            return CartDto.Empty(ownerId, request.OwnerType);

        var cart = await _repository.GetByOwnerAsync(ownerId, request.OwnerType, cancellationToken);

        return cart is null
            ? CartDto.Empty(ownerId, request.OwnerType)
            : CartDto.MapFromEntity(cart);
    }
}
