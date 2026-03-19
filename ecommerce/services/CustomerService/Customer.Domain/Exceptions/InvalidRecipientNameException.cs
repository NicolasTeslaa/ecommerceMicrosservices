using Customer.Domain.Enums;

namespace Customer.Domain.Exceptions;

public class InvalidRecipientNameException : CustomerException
{
    public InvalidRecipientNameException()
        : base(CustomerErrorCode.InvalidRecipientName, "Recipient name is required.")
    {
    }
}
