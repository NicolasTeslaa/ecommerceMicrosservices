using Cart.Domain.Enums;

namespace Cart.Domain.Exceptions;

public class InvalidProductIdException : CartException
{
    public InvalidProductIdException()
        : base(CartErrorCode.InvalidProductId, "Product id must be a valid non-empty value.")
    {
    }
}
