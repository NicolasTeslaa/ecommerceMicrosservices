namespace Shipping.Domain.Enums;

public enum ShippingErrorCode
{
    Unknown = 0,
    InvalidRequest = 1000,
    InvalidHeight = 1001,
    InvalidWidth = 1002,
    InvalidCubage = 1003,
    InvalidWeight = 1004,
    InvalidOriginZipCode = 1005,
    InvalidDestinationZipCode = 1006,
    ProviderNotSupported = 2001
}
