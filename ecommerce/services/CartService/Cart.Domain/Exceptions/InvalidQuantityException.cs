using Cart.Domain.Enums;

namespace Cart.Domain.Exceptions;

public class InvalidQuantityException : CartException
{
    public InvalidQuantityException()
        : base(CartErrorCode.InvalidQuantity, "Product quantity must be zero or greater.")
    {
    }
}
