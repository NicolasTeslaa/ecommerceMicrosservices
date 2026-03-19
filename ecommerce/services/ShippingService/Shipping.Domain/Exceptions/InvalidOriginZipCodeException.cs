using Shipping.Domain.Enums;

namespace Shipping.Domain.Exceptions;

public class InvalidOriginZipCodeException : ShippingException
{
    public InvalidOriginZipCodeException() : base(ShippingErrorCode.InvalidOriginZipCode, "Origin zip code is required.")
    {
    }
}
