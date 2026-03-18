using Cart.Application.DTOs;
using Cart.Application.Interfaces;
using Cart.Application.Queries;
using Cart.Domain.Exceptions;
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
            throw new InvalidOwnerIdException();

        if (!Enum.IsDefined(request.OwnerType))
            throw new InvalidOwnerTypeException();

        var cart = await _repository.GetByOwnerAsync(ownerId, request.OwnerType, cancellationToken);

        return cart is null
            ? CartDto.Empty(ownerId, request.OwnerType)
            : CartDto.MapFromEntity(cart);
    }
}
