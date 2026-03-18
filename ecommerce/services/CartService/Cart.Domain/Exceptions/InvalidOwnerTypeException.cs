using Cart.Domain.Enums;

namespace Cart.Domain.Exceptions;

public class InvalidOwnerTypeException : CartException
{
    public InvalidOwnerTypeException()
        : base(CartErrorCode.InvalidOwnerType, "Cart owner type is invalid.")
    {
    }
}
