using Customer.Domain.Enums;

namespace Customer.Domain.Exceptions;

public class InvalidStreetException : CustomerException
{
    public InvalidStreetException()
        : base(CustomerErrorCode.InvalidStreet, "Street is required.")
    {
    }
}
