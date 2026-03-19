namespace Customer.Application.DTOs;

public class CustomerAddressDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
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
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }

    public static CustomerAddressDto MapFromEntity(Customer.Domain.Entities.CustomerAddress address)
    {
        return new CustomerAddressDto
        {
            Id = address.Id,
            CustomerId = address.CustomerId,
            Label = address.Label,
            RecipientName = address.RecipientName,
            Street = address.Street,
            Number = address.Number,
            Complement = address.Complement,
            Neighborhood = address.Neighborhood,
            City = address.City,
            State = address.State,
            ZipCode = address.ZipCode,
            Country = address.Country,
            Reference = address.Reference,
            IsDefault = address.IsDefault,
            CreatedAtUtc = address.CreatedAtUtc,
            UpdatedAtUtc = address.UpdatedAtUtc
        };
    }
}
