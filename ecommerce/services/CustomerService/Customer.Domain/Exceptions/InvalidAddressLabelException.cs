using Customer.Domain.Enums;

namespace Customer.Domain.Exceptions;

public class InvalidAddressLabelException : CustomerException
{
    public InvalidAddressLabelException()
        : base(CustomerErrorCode.InvalidAddressLabel, "Address label is required.")
    {
    }
}
