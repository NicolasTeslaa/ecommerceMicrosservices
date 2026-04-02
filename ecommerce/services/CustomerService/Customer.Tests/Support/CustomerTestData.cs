using Customer.Application.Commands;
using Customer.Domain.Entities;

namespace Customer.Tests.Support;

internal static class CustomerTestData
{
    public static Customer.Domain.Entities.Customer CreateCustomer(
        Guid? id = null,
        Guid? authUserId = null,
        string fullName = "Jane Doe",
        string email = "jane@example.com",
        string phoneNumber = "11999999999")
    {
        return new Customer.Domain.Entities.Customer(
            id ?? Guid.NewGuid(),
            authUserId ?? Guid.NewGuid(),
            fullName,
            email,
            phoneNumber,
            DateTime.UtcNow);
    }

    public static CustomerAddress AddAddress(Customer.Domain.Entities.Customer customer, bool isDefault = false)
    {
        return customer.AddAddress(
            "Casa",
            "Jane Doe",
            "Rua A",
            "123",
            "Apto 1",
            "Centro",
            "Sao Paulo",
            "SP",
            "01000-000",
            "Brasil",
            "Proximo a padaria",
            isDefault);
    }

    public static UpsertCustomerAddressCommand CreateUpsertCommand(Guid? customerId = null, Guid? addressId = null, bool isDefault = false)
    {
        return new UpsertCustomerAddressCommand
        {
            CustomerId = customerId ?? Guid.NewGuid(),
            AddressId = addressId,
            Label = "Casa",
            RecipientName = "Jane Doe",
            Street = "Rua A",
            Number = "123",
            Complement = "Apto 1",
            Neighborhood = "Centro",
            City = "Sao Paulo",
            State = "SP",
            ZipCode = "01000-000",
            Country = "Brasil",
            Reference = "Portao azul",
            IsDefault = isDefault
        };
    }
}
