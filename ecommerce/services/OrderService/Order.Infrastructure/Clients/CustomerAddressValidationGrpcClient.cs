using Grpc.Core;
using Order.Application.DTOs;
using Order.Application.Interfaces;
using Order.Domain.Exceptions;
using ECommerce.Shared.Protos;

namespace Order.Infrastructure.Clients;

public class CustomerAddressValidationGrpcClient : ICustomerAddressValidationClient
{
    private readonly CustomerAddressValidation.CustomerAddressValidationClient _client;

    public CustomerAddressValidationGrpcClient(CustomerAddressValidation.CustomerAddressValidationClient client)
    {
        _client = client;
    }

    public async Task<ValidatedCustomerAddressDto> ValidateAsync(Guid customerId, Guid customerAddressId, CancellationToken cancellationToken = default)
    {
        try
        {
            var reply = await _client.ValidateAddressAsync(
                new ValidateCustomerAddressRequest
                {
                    CustomerId = customerId.ToString(),
                    AddressId = customerAddressId.ToString()
                },
                cancellationToken: cancellationToken);

            return new ValidatedCustomerAddressDto
            {
                CustomerId = Guid.Parse(reply.CustomerId),
                AddressId = Guid.Parse(reply.AddressId),
                CustomerEmail = reply.CustomerEmail,
                FormattedAddress = reply.FormattedAddress
            };
        }
        catch (RpcException exception) when (exception.StatusCode == StatusCode.NotFound)
        {
            throw new CustomerAddressNotFoundException(customerId, customerAddressId);
        }
    }
}
