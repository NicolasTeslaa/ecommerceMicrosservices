using Customer.Application.Interfaces;
using Customer.Domain.Exceptions;
using ECommerce.Shared.Protos;
using Grpc.Core;

namespace Customer.API.Grpc;

public class CustomerAddressValidationGrpcService : CustomerAddressValidation.CustomerAddressValidationBase
{
    private readonly ICustomerRepository _repository;

    public CustomerAddressValidationGrpcService(ICustomerRepository repository)
    {
        _repository = repository;
    }

    public override async Task<ValidateCustomerAddressReply> ValidateAddress(ValidateCustomerAddressRequest request, ServerCallContext context)
    {
        var customerId = Guid.Parse(request.CustomerId);
        var addressId = Guid.Parse(request.AddressId);

        var customer = await _repository.GetByIdAsync(customerId, context.CancellationToken)
            ?? throw new RpcException(new Status(StatusCode.NotFound, $"Customer '{customerId}' was not found."));

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
            throw new RpcException(new Status(StatusCode.NotFound, exception.Message));
        }
    }
}
