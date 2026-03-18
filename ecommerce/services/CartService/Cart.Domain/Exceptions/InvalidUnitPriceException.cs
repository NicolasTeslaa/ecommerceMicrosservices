using Cart.Domain.Enums;

namespace Cart.Domain.Exceptions;

public class InvalidUnitPriceException : CartException
{
    public InvalidUnitPriceException()
        : base(CartErrorCode.InvalidUnitPrice, "Product unit price must be greater than zero.")
    {
    }
}
