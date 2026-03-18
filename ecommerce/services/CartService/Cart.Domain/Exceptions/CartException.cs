using Cart.Domain.Enums;

namespace Cart.Domain.Exceptions;

public abstract class CartException : Exception
{
    protected CartException(CartErrorCode errorCode, string message, Exception? innerException = null)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }

    public CartErrorCode ErrorCode { get; }
}
