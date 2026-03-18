using Cart.Application.DTOs;
using Cart.Domain.Enums;
using MediatR;

namespace Cart.Application.Queries;

public class GetCartQuery : IRequest<CartDto>
{
    public GetCartQuery(string ownerId, CartOwnerType ownerType)
    {
        OwnerId = ownerId;
        OwnerType = ownerType;
    }

    public string OwnerId { get; }
    public CartOwnerType OwnerType { get; }
}
