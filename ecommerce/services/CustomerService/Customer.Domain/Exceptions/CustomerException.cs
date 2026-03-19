using Customer.Domain.Enums;

namespace Customer.Domain.Exceptions;

public abstract class CustomerException : Exception
{
    protected CustomerException(CustomerErrorCode errorCode, string message, Exception? innerException = null)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }

    public CustomerErrorCode ErrorCode { get; }
}
