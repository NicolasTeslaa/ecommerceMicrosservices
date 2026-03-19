using Order.Application.DTOs;

namespace Order.Application.Interfaces;

public interface ICustomerAddressValidationClient
{
    Task<ValidatedCustomerAddressDto> ValidateAsync(Guid customerId, Guid customerAddressId, CancellationToken cancellationToken = default);
}
