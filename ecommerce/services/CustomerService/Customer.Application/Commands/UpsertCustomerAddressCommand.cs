using System.ComponentModel.DataAnnotations;
using Customer.Application.DTOs;
using MediatR;

namespace Customer.Application.Commands;

public class UpsertCustomerAddressCommand : IRequest<CustomerAddressDto>
{
    [Required]
    public Guid CustomerId { get; set; }

    public Guid? AddressId { get; set; }

    [Required]
    [MaxLength(50)]
    public string Label { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string RecipientName { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string Street { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Number { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Complement { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Neighborhood { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string City { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string State { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string ZipCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Country { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Reference { get; set; } = string.Empty;

    public bool IsDefault { get; set; }
}
