namespace Catalog.Domain.Enums;

public enum CatalogErrorCode
{
    Unknown = 0,
    InvalidRequest = 1000,
    InvalidProductId = 1001,
    InvalidProductName = 1002,
    InvalidProductPrice = 1003,
    InvalidStockQuantity = 1004,
    InvalidCategoryId = 1005,
    InvalidCategoryName = 1006,
    InvalidProductHeight = 1007,
    InvalidProductWidth = 1008,
    InvalidProductCubage = 1009,
    InvalidProductWeight = 1010,
    InvalidProductOriginZipCode = 1011,
    ProductNotFound = 2001,
    CategoryNotFound = 2002,
    PersistenceFailure = 3001
}
