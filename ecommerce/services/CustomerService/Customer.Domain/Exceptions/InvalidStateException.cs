using Customer.Domain.Enums;

namespace Customer.Domain.Exceptions;

public class InvalidStateException : CustomerException
{
    public InvalidStateException()
        : base(CustomerErrorCode.InvalidState, "State is required.")
    {
    }
}
