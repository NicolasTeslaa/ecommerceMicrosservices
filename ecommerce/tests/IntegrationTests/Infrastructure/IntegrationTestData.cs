using IntegrationTests.Contracts;

namespace IntegrationTests.Infrastructure;

internal static class IntegrationTestData
{
    public static RegisterUserRequest CreateRegisterUserRequest(string suffix)
    {
        return new RegisterUserRequest
        {
            FullName = $"Integration User {suffix}",
            Email = $"integration.{suffix}@example.com",
            PhoneNumber = "(11) 98888-7766",
            Password = "Secret123!"
        };
    }

    public static UpsertCustomerAddressRequest CreateAddressRequest(bool isDefault = true)
    {
        return new UpsertCustomerAddressRequest
        {
            Label = "Casa",
            RecipientName = "Integration User",
            Street = "Rua das Integracoes",
            Number = "123",
            Complement = "Apto 45",
            Neighborhood = "Centro",
            City = "Sao Paulo",
            State = "SP",
            ZipCode = "01001-000",
            Country = "Brasil",
            Reference = "Portaria principal",
            IsDefault = isDefault
        };
    }

    public static string NormalizePhoneNumber(string value)
    {
        return new string(value.Where(char.IsDigit).ToArray());
    }
}
