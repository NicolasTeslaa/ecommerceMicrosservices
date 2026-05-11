using Customer.Application.Commands;
using Customer.Application.DTOs;
using Customer.Application.Interfaces;
using MediatR;

namespace Customer.Application.Handlers;

public class UpsertCustomerAddressHandler : IRequestHandler<UpsertCustomerAddressCommand, CustomerAddressDto>
{
    private readonly ICustomerRepository _repository;

    public UpsertCustomerAddressHandler(ICustomerRepository repository)
    {
        _repository = repository;
    }

    public async Task<CustomerAddressDto> Handle(UpsertCustomerAddressCommand request, CancellationToken cancellationToken)
    {
        var customer = await _repository.GetByIdAsync(request.CustomerId, cancellationToken);
        if (customer is null)
            return new CustomerAddressDto
            {
                CustomerId = request.CustomerId,
                Id = request.AddressId ?? Guid.Empty,
                Label = request.Label ?? string.Empty,
                RecipientName = request.RecipientName ?? string.Empty
            };

        var address = request.AddressId.HasValue && request.AddressId.Value != Guid.Empty
            ? customer.UpdateAddress(
                request.AddressId.Value,
                request.Label,
                request.RecipientName,
                request.Street,
                request.Number,
                request.Complement,
                request.Neighborhood,
                request.City,
                request.State,
                request.ZipCode,
                request.Country,
                request.Reference,
                request.IsDefault)
            : customer.AddAddress(
                request.Label,
                request.RecipientName,
                request.Street,
                request.Number,
                request.Complement,
                request.Neighborhood,
                request.City,
                request.State,
                request.ZipCode,
                request.Country,
                request.Reference,
                request.IsDefault);

        await _repository.UpdateAsync(customer, cancellationToken);
        return CustomerAddressDto.MapFromEntity(address);
    }
}
