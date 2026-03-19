using Shipping.Domain.Enums;

namespace Shipping.Domain.Exceptions;

public class InvalidCubageException : ShippingException
{
    public InvalidCubageException() : base(ShippingErrorCode.InvalidCubage, "Cubage must be greater than zero.")
    {
    }
}
