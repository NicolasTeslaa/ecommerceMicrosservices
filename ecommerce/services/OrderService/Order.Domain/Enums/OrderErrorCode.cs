namespace Order.Domain.Enums;

public enum OrderErrorCode
{
    Unknown = 0,
    InvalidRequest = 1,
    InvalidOrderId = 2,
    InvalidCustomerId = 3,
    InvalidCustomerEmail = 4,
    InvalidShippingAddress = 5,
    InvalidPaymentMethod = 6,
    InvalidCustomerAddressId = 7,
    InvalidOrderItem = 8,
    InvalidProductId = 9,
    InvalidProductName = 10,
    InvalidUnitPrice = 11,
    InvalidQuantity = 12,
    OrderNotFound = 13,
    CustomerAddressNotFound = 14,
    PersistenceError = 15
}
