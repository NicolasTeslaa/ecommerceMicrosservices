namespace Customer.Domain.Enums;

public enum CustomerErrorCode
{
    Unknown = 0,
    InvalidRequest = 1000,
    InvalidCustomerEmail = 1001,
    InvalidCustomerName = 1002,
    InvalidAddressLabel = 1003,
    InvalidRecipientName = 1004,
    InvalidStreet = 1005,
    InvalidNumber = 1006,
    InvalidNeighborhood = 1007,
    InvalidCity = 1008,
    InvalidState = 1009,
    InvalidZipCode = 1010,
    InvalidCountry = 1011,
    CustomerNotFound = 2001,
    CustomerAddressNotFound = 2002,
    PersistenceFailure = 3001
}
