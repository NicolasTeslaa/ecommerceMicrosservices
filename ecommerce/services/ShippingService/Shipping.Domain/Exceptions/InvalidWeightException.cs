using Shipping.Domain.Enums;

namespace Shipping.Domain.Exceptions;

public class InvalidWeightException : ShippingException
{
    public InvalidWeightException() : base(ShippingErrorCode.InvalidWeight, "Weight must be greater than zero.")
    {
    }
}
