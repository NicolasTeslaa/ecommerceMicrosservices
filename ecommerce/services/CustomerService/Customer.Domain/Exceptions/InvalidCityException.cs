using Customer.Domain.Enums;

namespace Customer.Domain.Exceptions;

public class InvalidCityException : CustomerException
{
    public InvalidCityException()
        : base(CustomerErrorCode.InvalidCity, "City is required.")
    {
    }
}
