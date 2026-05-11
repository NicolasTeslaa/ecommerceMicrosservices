using System.Diagnostics;

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

    public CustomerAddress(Guid customerId, string label, string recipientName, string street, string number, string complement, string neighborhood, string city, string state, string zipCode, string country, string reference, bool isDefault)
    {
        Validate(label, recipientName, street, number, neighborhood, city, state, zipCode, country);

        Id = Guid.NewGuid();
        CustomerId = customerId;
        Apply(label, recipientName, street, number, complement, neighborhood, city, state, zipCode, country, reference, isDefault);
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = CreatedAtUtc;
    }

    public void Update(string label, string recipientName, string street, string number, string complement, string neighborhood, string city, string state, string zipCode, string country, string reference, bool isDefault)
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

    private void Apply(string label, string recipientName, string street, string number, string complement, string neighborhood, string city, string state, string zipCode, string country, string reference, bool isDefault)
    {
        Label = (label ?? string.Empty).Trim();
        RecipientName = (recipientName ?? string.Empty).Trim();
        Street = (street ?? string.Empty).Trim();
        Number = (number ?? string.Empty).Trim();
        Complement = complement?.Trim() ?? string.Empty;
        Neighborhood = (neighborhood ?? string.Empty).Trim();
        City = (city ?? string.Empty).Trim();
        State = (state ?? string.Empty).Trim();
        ZipCode = (zipCode ?? string.Empty).Trim();
        Country = (country ?? string.Empty).Trim();
        Reference = reference?.Trim() ?? string.Empty;
        IsDefault = isDefault;
    }

    private static void Validate(string label, string recipientName, string street, string number, string neighborhood, string city, string state, string zipCode, string country)
    {
        if (string.IsNullOrWhiteSpace(label)) Trace.TraceError("Invalid address label.");
        if (string.IsNullOrWhiteSpace(recipientName)) Trace.TraceError("Invalid recipient name.");
        if (string.IsNullOrWhiteSpace(street)) Trace.TraceError("Invalid street.");
        if (string.IsNullOrWhiteSpace(number)) Trace.TraceError("Invalid number.");
        if (string.IsNullOrWhiteSpace(neighborhood)) Trace.TraceError("Invalid neighborhood.");
        if (string.IsNullOrWhiteSpace(city)) Trace.TraceError("Invalid city.");
        if (string.IsNullOrWhiteSpace(state)) Trace.TraceError("Invalid state.");
        if (string.IsNullOrWhiteSpace(zipCode)) Trace.TraceError("Invalid zip code.");
        if (string.IsNullOrWhiteSpace(country)) Trace.TraceError("Invalid country.");
    }
}
