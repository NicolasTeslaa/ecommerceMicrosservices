using Cart.Domain.Enums;

namespace Cart.Domain.Exceptions;

public class CartNotFoundException : CartException
{
    public CartNotFoundException(string ownerId, CartOwnerType ownerType)
        : base(CartErrorCode.CartNotFound, $"Cart for owner '{ownerType}:{ownerId}' was not found.")
    {
    }
}
