using Customer.Domain.Enums;

namespace Customer.Domain.Exceptions;

public class InvalidCustomerEmailException : CustomerException
{
    public InvalidCustomerEmailException()
        : base(CustomerErrorCode.InvalidCustomerEmail, "A valid customer email must be provided.")
    {
    }
}
