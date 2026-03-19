namespace Order.Application.DTOs;

public class CustomerProfileDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public IReadOnlyCollection<CustomerAddressProfileDto> Addresses { get; set; } = Array.Empty<CustomerAddressProfileDto>();
}

public class CustomerAddressProfileDto
{
    public Guid Id { get; set; }
    public string Label { get; set; } = string.Empty;
    public string RecipientName { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public string Complement { get; set; } = string.Empty;
    public string Neighborhood { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public bool IsDefault { get; set; }

    public string ToSingleLine()
    {
        var complement = string.IsNullOrWhiteSpace(Complement) ? string.Empty : $", {Complement}";
        var reference = string.IsNullOrWhiteSpace(Reference) ? string.Empty : $" ({Reference})";
        return $"{RecipientName} - {Street}, {Number}{complement}, {Neighborhood}, {City}/{State}, {ZipCode}, {Country}{reference}";
    }
}
