using Customer.Domain.Exceptions;

namespace Customer.Domain.Entities;

public class CustomerAddress
{
    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public string Label { get; private set; } = string.Empty;
    public string RecipientName { get; private set; } = string.Empty;
    public string Street { get; private set; } = string.Empty;
    public string Number { get; private set; } = string.Empty;
    public string Complement { get; private set; } = string.Empty;
    public string Neighborhood { get; private set; } = string.Empty;
    public string City { get; private set; } = string.Empty;
    public string State { get; private set; } = string.Empty;
    public string ZipCode { get; private set; } = string.Empty;
    public string Country { get; private set; } = string.Empty;
    public string Reference { get; private set; } = string.Empty;
    public bool IsDefault { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    private CustomerAddress()
    {
    }

    public CustomerAddress(
        Guid customerId,
        string label,
        string recipientName,
        string street,
        string number,
        string complement,
        string neighborhood,
        string city,
        string state,
        string zipCode,
        string country,
        string reference,
        bool isDefault)
    {
        Validate(label, recipientName, street, number, neighborhood, city, state, zipCode, country);

        Id = Guid.NewGuid();
        CustomerId = customerId;
        Apply(label, recipientName, street, number, complement, neighborhood, city, state, zipCode, country, reference, isDefault);
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = CreatedAtUtc;
    }

    public void Update(
        string label,
        string recipientName,
        string street,
        string number,
        string complement,
        string neighborhood,
        string city,
        string state,
        string zipCode,
        string country,
        string reference,
        bool isDefault)
    {
        Validate(label, recipientName, street, number, neighborhood, city, state, zipCode, country);
        Apply(label, recipientName, street, number, complement, neighborhood, city, state, zipCode, country, reference, isDefault);
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void SetDefault(bool isDefault)
    {
        IsDefault = isDefault;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public string ToSingleLine()
    {
        var complement = string.IsNullOrWhiteSpace(Complement) ? string.Empty : $", {Complement}";
        var reference = string.IsNullOrWhiteSpace(Reference) ? string.Empty : $" ({Reference})";
        return $"{RecipientName} - {Street}, {Number}{complement}, {Neighborhood}, {City}/{State}, {ZipCode}, {Country}{reference}";
    }

    private void Apply(
        string label,
        string recipientName,
        string street,
        string number,
        string complement,
        string neighborhood,
        string city,
        string state,
        string zipCode,
        string country,
        string reference,
        bool isDefault)
    {
        Label = label.Trim();
        RecipientName = recipientName.Trim();
        Street = street.Trim();
        Number = number.Trim();
        Complement = complement?.Trim() ?? string.Empty;
        Neighborhood = neighborhood.Trim();
        City = city.Trim();
        State = state.Trim();
        ZipCode = zipCode.Trim();
        Country = country.Trim();
        Reference = reference?.Trim() ?? string.Empty;
        IsDefault = isDefault;
    }

    private static void Validate(
        string label,
        string recipientName,
        string street,
        string number,
        string neighborhood,
        string city,
        string state,
        string zipCode,
        string country)
    {
        if (string.IsNullOrWhiteSpace(label))
            throw new InvalidAddressLabelException();
        if (string.IsNullOrWhiteSpace(recipientName))
            throw new InvalidRecipientNameException();
        if (string.IsNullOrWhiteSpace(street))
            throw new InvalidStreetException();
        if (string.IsNullOrWhiteSpace(number))
            throw new InvalidNumberException();
        if (string.IsNullOrWhiteSpace(neighborhood))
            throw new InvalidNeighborhoodException();
        if (string.IsNullOrWhiteSpace(city))
            throw new InvalidCityException();
        if (string.IsNullOrWhiteSpace(state))
            throw new InvalidStateException();
        if (string.IsNullOrWhiteSpace(zipCode))
            throw new InvalidZipCodeException();
        if (string.IsNullOrWhiteSpace(country))
            throw new InvalidCountryException();
    }
}
