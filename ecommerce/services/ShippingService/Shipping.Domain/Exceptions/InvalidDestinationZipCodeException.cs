using Shipping.Domain.Enums;

namespace Shipping.Domain.Exceptions;

public class InvalidDestinationZipCodeException : ShippingException
{
    public InvalidDestinationZipCodeException() : base(ShippingErrorCode.InvalidDestinationZipCode, "Destination zip code is required.")
    {
    }
}
