using Shipping.Domain.Enums;

namespace Shipping.Domain.Exceptions;

public abstract class ShippingException : Exception
{
    protected ShippingException(ShippingErrorCode errorCode, string message) : base(message)
    {
        ErrorCode = errorCode;
    }

    public ShippingErrorCode ErrorCode { get; }
}
