using Cart.Domain.Enums;

namespace Cart.Domain.Exceptions;

public class CartItemNotFoundException : CartException
{
    public CartItemNotFoundException(Guid productId)
        : base(CartErrorCode.CartItemNotFound, $"Cart item for product '{productId}' was not found.")
    {
    }
}
