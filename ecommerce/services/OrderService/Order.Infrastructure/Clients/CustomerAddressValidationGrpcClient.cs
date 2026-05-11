using Grpc.Core;
using Order.Application.DTOs;
using Order.Application.Interfaces;
using ECommerce.Shared.Protos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Order.Infrastructure.Clients;

public class CustomerAddressValidationGrpcClient : ICustomerAddressValidationClient
{
    private readonly CustomerAddressValidation.CustomerAddressValidationClient _client;
    private readonly ILogger<CustomerAddressValidationGrpcClient> _logger;

    public CustomerAddressValidationGrpcClient(CustomerAddressValidation.CustomerAddressValidationClient client, ILogger<CustomerAddressValidationGrpcClient>? logger = null)
    {
        _client = client;
        _logger = logger ?? NullLogger<CustomerAddressValidationGrpcClient>.Instance;
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
            _logger.LogError(exception, "Customer address '{AddressId}' for customer '{CustomerId}' was not found.", customerAddressId, customerId);
            return new ValidatedCustomerAddressDto
            {
                CustomerId = customerId,
                AddressId = customerAddressId,
                CustomerEmail = string.Empty,
                FormattedAddress = string.Empty
            };
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to validate customer address '{AddressId}' for customer '{CustomerId}'.", customerAddressId, customerId);
            return new ValidatedCustomerAddressDto
            {
                CustomerId = customerId,
                AddressId = customerAddressId,
                CustomerEmail = string.Empty,
                FormattedAddress = string.Empty
            };
        }
    }
}
