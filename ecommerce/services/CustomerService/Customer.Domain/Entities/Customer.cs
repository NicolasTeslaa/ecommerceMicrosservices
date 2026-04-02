using System.Net.Mail;
using Customer.Domain.Exceptions;

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
        FullName = fullName.Trim();
        Email = email.Trim().ToLowerInvariant();
        PhoneNumber = NormalizePhoneNumber(phoneNumber);
        CreatedAtUtc = createdAtUtc;
    }

    public CustomerAddress AddAddress(
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
        if (!_addresses.Any())
            isDefault = true;

        if (isDefault)
            ClearDefaultAddress();

        var address = new CustomerAddress(
            Id,
            label,
            recipientName,
            street,
            number,
            complement,
            neighborhood,
            city,
            state,
            zipCode,
            country,
            reference,
            isDefault);

        _addresses.Add(address);
        return address;
    }

    public CustomerAddress UpdateAddress(
        Guid addressId,
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
        var address = GetAddressOrThrow(addressId);

        if (isDefault)
            ClearDefaultAddress();

        address.Update(label, recipientName, street, number, complement, neighborhood, city, state, zipCode, country, reference, isDefault);
        return address;
    }

    public void RemoveAddress(Guid addressId)
    {
        var address = GetAddressOrThrow(addressId);
        var wasDefault = address.IsDefault;
        _addresses.Remove(address);

        if (wasDefault && _addresses.Count > 0)
            _addresses[0].SetDefault(true);
    }

    public CustomerAddress SetDefaultAddress(Guid addressId)
    {
        var address = GetAddressOrThrow(addressId);
        ClearDefaultAddress();
        address.SetDefault(true);
        return address;
    }

    public CustomerAddress GetAddress(Guid addressId) => GetAddressOrThrow(addressId);

    private CustomerAddress GetAddressOrThrow(Guid addressId)
    {
        var address = _addresses.FirstOrDefault(item => item.Id == addressId);
        return address ?? throw new CustomerAddressNotFoundException(Id, addressId);
    }

    private void ClearDefaultAddress()
    {
        foreach (var address in _addresses.Where(item => item.IsDefault))
            address.SetDefault(false);
    }

    private static void Validate(string fullName, string email, string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new InvalidCustomerNameException();

        if (string.IsNullOrWhiteSpace(email))
            throw new InvalidCustomerEmailException();

        try
        {
            _ = new MailAddress(email);
        }
        catch (FormatException)
        {
            throw new InvalidCustomerEmailException();
        }

        var digits = new string(phoneNumber.Where(char.IsDigit).ToArray());
        if (digits.Length < 10 || digits.Length > 15)
            throw new InvalidOperationException("A valid customer phone number must be provided.");
    }

    private static string NormalizePhoneNumber(string phoneNumber)
    {
        return new string(phoneNumber.Where(char.IsDigit).ToArray());
    }
}
