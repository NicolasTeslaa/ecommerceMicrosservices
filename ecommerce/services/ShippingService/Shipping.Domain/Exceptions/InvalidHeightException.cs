using Shipping.Domain.Enums;

namespace Shipping.Domain.Exceptions;

public class InvalidHeightException : ShippingException
{
    public InvalidHeightException() : base(ShippingErrorCode.InvalidHeight, "Height must be greater than zero.")
    {
    }
}
