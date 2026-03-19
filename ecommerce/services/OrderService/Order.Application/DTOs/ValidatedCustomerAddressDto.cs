namespace Order.Application.DTOs;

public class ValidatedCustomerAddressDto
{
    public Guid CustomerId { get; set; }
    public Guid AddressId { get; set; }
    public string CustomerEmail { get; set; } = string.Empty;
    public string FormattedAddress { get; set; } = string.Empty;
}
