using Shipping.Domain.Enums;

namespace Shipping.Domain.Exceptions;

public class InvalidWidthException : ShippingException
{
    public InvalidWidthException() : base(ShippingErrorCode.InvalidWidth, "Width must be greater than zero.")
    {
    }
}
