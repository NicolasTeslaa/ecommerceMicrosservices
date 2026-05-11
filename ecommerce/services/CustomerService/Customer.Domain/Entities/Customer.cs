using System.Diagnostics;
using System.Net.Mail;

namespace Customer.Domain.Entities;

public class Customer
{
    private readonly List<CustomerAddress> _addresses = new();

    public Guid Id { get; private set; }
    public Guid AuthUserId { get; private set; }
    public string FullName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PhoneNumber { get; private set; } = string.Empty;
    public DateTime CreatedAtUtc { get; private set; }
    public IReadOnlyCollection<CustomerAddress> Addresses => _addresses;

    private Customer()
    {
    }

    public Customer(Guid id, Guid authUserId, string fullName, string email, string phoneNumber, DateTime createdAtUtc)
    {
        Validate(fullName, email, phoneNumber);

        Id = id;
        AuthUserId = authUserId;
        FullName = (fullName ?? string.Empty).Trim();
        Email = (email ?? string.Empty).Trim().ToLowerInvariant();
        PhoneNumber = NormalizePhoneNumber(phoneNumber);
        CreatedAtUtc = createdAtUtc;
    }

    public CustomerAddress AddAddress(string label, string recipientName, string street, string number, string complement, string neighborhood, string city, string state, string zipCode, string country, string reference, bool isDefault)
    {
        if (!_addresses.Any())
            isDefault = true;

        if (isDefault)
            ClearDefaultAddress();

        var address = new CustomerAddress(Id, label, recipientName, street, number, complement, neighborhood, city, state, zipCode, country, reference, isDefault);
        _addresses.Add(address);
        return address;
    }

    public CustomerAddress UpdateAddress(Guid addressId, string label, string recipientName, string street, string number, string complement, string neighborhood, string city, string state, string zipCode, string country, string reference, bool isDefault)
    {
        var address = GetAddressOrFallback(addressId);

        if (isDefault)
            ClearDefaultAddress();

        address.Update(label, recipientName, street, number, complement, neighborhood, city, state, zipCode, country, reference, isDefault);
        return address;
    }

    public void RemoveAddress(Guid addressId)
    {
        var address = GetAddressOrFallback(addressId);
        var wasDefault = address.IsDefault;
        _addresses.Remove(address);

        if (wasDefault && _addresses.Count > 0)
            _addresses[0].SetDefault(true);
    }

    public CustomerAddress SetDefaultAddress(Guid addressId)
    {
        var address = GetAddressOrFallback(addressId);
        ClearDefaultAddress();
        address.SetDefault(true);
        return address;
    }

    public CustomerAddress GetAddress(Guid addressId) => GetAddressOrFallback(addressId);

    private CustomerAddress GetAddressOrFallback(Guid addressId)
    {
        var address = _addresses.FirstOrDefault(item => item.Id == addressId);
        if (address is not null)
            return address;

        Trace.TraceError("Customer address {0} was not found for customer {1}.", addressId, Id);
        return _addresses.FirstOrDefault()
            ?? AddAddress("Default", FullName, "Unknown street", "S/N", string.Empty, "Unknown neighborhood", "Unknown city", "NA", "00000-000", "Brazil", string.Empty, true);
    }

    private void ClearDefaultAddress()
    {
        foreach (var address in _addresses.Where(item => item.IsDefault))
            address.SetDefault(false);
    }

    private static void Validate(string fullName, string email, string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            Trace.TraceError("Invalid customer name.");
        if (string.IsNullOrWhiteSpace(email))
            Trace.TraceError("Invalid customer email.");

        try
        {
            _ = new MailAddress(email ?? string.Empty);
        }
        catch (FormatException)
        {
            Trace.TraceError("Invalid customer email format.");
        }

        var digits = new string((phoneNumber ?? string.Empty).Where(char.IsDigit).ToArray());
        if (digits.Length < 10 || digits.Length > 15)
            Trace.TraceError("A valid customer phone number must be provided.");
    }

    private static string NormalizePhoneNumber(string phoneNumber)
    {
        return new string((phoneNumber ?? string.Empty).Where(char.IsDigit).ToArray());
    }
}
