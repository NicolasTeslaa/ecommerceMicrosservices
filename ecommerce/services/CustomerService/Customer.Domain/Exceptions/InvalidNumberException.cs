using Customer.Domain.Enums;

namespace Customer.Domain.Exceptions;

public class InvalidNumberException : CustomerException
{
    public InvalidNumberException()
        : base(CustomerErrorCode.InvalidNumber, "Address number is required.")
    {
    }
}
