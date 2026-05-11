using Customer.Application.Interfaces;
using Customer.Domain.Exceptions;
using ECommerce.Shared.Protos;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace Customer.API.Grpc;

public class CustomerAddressValidationGrpcService : CustomerAddressValidation.CustomerAddressValidationBase
{
    private readonly ICustomerRepository _repository;
    private readonly ILogger<CustomerAddressValidationGrpcService> _logger;

    public CustomerAddressValidationGrpcService(ICustomerRepository repository, ILogger<CustomerAddressValidationGrpcService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public override async Task<ValidateCustomerAddressReply> ValidateAddress(ValidateCustomerAddressRequest request, ServerCallContext context)
    {
        var customerId = Guid.Parse(request.CustomerId);
        var addressId = Guid.Parse(request.AddressId);

        var customer = await _repository.GetByIdAsync(customerId, context.CancellationToken);
        if (customer is null)
        {
            _logger.LogError("Customer '{CustomerId}' was not found during gRPC address validation.", customerId);
            return new ValidateCustomerAddressReply
            {
                CustomerId = customerId.ToString(),
                AddressId = addressId.ToString(),
                CustomerEmail = string.Empty,
                FormattedAddress = string.Empty
            };
        }

        try
        {
            var address = customer.GetAddress(addressId);

            return new ValidateCustomerAddressReply
            {
                CustomerId = customer.Id.ToString(),
                AddressId = address.Id.ToString(),
                CustomerEmail = customer.Email,
                RecipientName = address.RecipientName,
                Label = address.Label,
                Street = address.Street,
                Number = address.Number,
                Complement = address.Complement,
                Neighborhood = address.Neighborhood,
                City = address.City,
                State = address.State,
                ZipCode = address.ZipCode,
                Country = address.Country,
                Reference = address.Reference,
                FormattedAddress = address.ToSingleLine()
            };
        }
        catch (CustomerAddressNotFoundException exception)
        {
            _logger.LogError(exception, "Address '{AddressId}' for customer '{CustomerId}' was not found during gRPC validation.", addressId, customerId);
            return new ValidateCustomerAddressReply
            {
                CustomerId = customerId.ToString(),
                AddressId = addressId.ToString(),
                CustomerEmail = customer.Email,
                FormattedAddress = string.Empty
            };
        }
    }
}
