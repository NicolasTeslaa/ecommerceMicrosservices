using Cart.Domain.Enums;

namespace Cart.Domain.Exceptions;

public class InvalidProductNameException : CartException
{
    public InvalidProductNameException()
        : base(CartErrorCode.InvalidProductName, "Product name must be provided.")
    {
    }
}
