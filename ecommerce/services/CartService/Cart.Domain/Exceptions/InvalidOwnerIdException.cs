using Cart.Domain.Enums;

namespace Cart.Domain.Exceptions;

public class InvalidOwnerIdException : CartException
{
    public InvalidOwnerIdException()
        : base(CartErrorCode.InvalidOwnerId, "Cart owner id must be provided.")
    {
    }
}
