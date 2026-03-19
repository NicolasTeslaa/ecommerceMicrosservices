using Shipping.Domain.Enums;

namespace Shipping.Domain.Exceptions;

public class ProviderNotSupportedException : ShippingException
{
    public ProviderNotSupportedException(string provider)
        : base(ShippingErrorCode.ProviderNotSupported, $"Shipping provider '{provider}' is not supported.")
    {
    }
}
