using Customer.Domain.Enums;

namespace Customer.Domain.Exceptions;

public class InvalidZipCodeException : CustomerException
{
    public InvalidZipCodeException()
        : base(CustomerErrorCode.InvalidZipCode, "Zip code is required.")
    {
    }
}
