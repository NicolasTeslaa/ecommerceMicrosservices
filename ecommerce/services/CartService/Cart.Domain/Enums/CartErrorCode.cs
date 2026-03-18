namespace Cart.Domain.Enums;

public enum CartErrorCode
{
    Unknown = 0,
    InvalidRequest = 1000,
    InvalidOwnerId = 1001,
    InvalidProductId = 1002,
    InvalidProductName = 1003,
    InvalidUnitPrice = 1004,
    InvalidQuantity = 1005,
    InvalidOwnerType = 1006,
    CartNotFound = 2001,
    CartItemNotFound = 2002,
    PersistenceFailure = 3001
}
