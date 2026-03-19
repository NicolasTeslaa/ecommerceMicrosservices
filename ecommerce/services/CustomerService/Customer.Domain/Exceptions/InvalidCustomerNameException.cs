using Customer.Domain.Enums;

namespace Customer.Domain.Exceptions;

public class InvalidCustomerNameException : CustomerException
{
    public InvalidCustomerNameException()
        : base(CustomerErrorCode.InvalidCustomerName, "Customer full name must be provided.")
    {
    }
}
