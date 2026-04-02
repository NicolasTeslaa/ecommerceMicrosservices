using Customer.Domain.Entities;

namespace Customer.Application.DTOs;

public class CustomerDto
{
    public Guid Id { get; set; }
    public Guid AuthUserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public IReadOnlyCollection<CustomerAddressDto> Addresses { get; set; } = Array.Empty<CustomerAddressDto>();

    public static CustomerDto MapFromEntity(Customer.Domain.Entities.Customer customer)
    {
        return new CustomerDto
        {
            Id = customer.Id,
            AuthUserId = customer.AuthUserId,
            FullName = customer.FullName,
            Email = customer.Email,
            PhoneNumber = customer.PhoneNumber,
            CreatedAtUtc = customer.CreatedAtUtc,
            Addresses = customer.Addresses
                .Select(CustomerAddressDto.MapFromEntity)
                .ToArray()
        };
    }
}
