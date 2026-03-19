using Customer.Domain.Enums;

namespace Customer.Domain.Exceptions;

public class InvalidCountryException : CustomerException
{
    public InvalidCountryException()
        : base(CustomerErrorCode.InvalidCountry, "Country is required.")
    {
    }
}
