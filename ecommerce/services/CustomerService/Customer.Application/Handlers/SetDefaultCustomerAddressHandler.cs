using Customer.Application.Commands;
using Customer.Application.DTOs;
using Customer.Application.Interfaces;
using MediatR;

namespace Customer.Application.Handlers;

public class SetDefaultCustomerAddressHandler : IRequestHandler<SetDefaultCustomerAddressCommand, CustomerAddressDto>
{
    private readonly ICustomerRepository _repository;

    public SetDefaultCustomerAddressHandler(ICustomerRepository repository)
    {
        _repository = repository;
    }

    public async Task<CustomerAddressDto> Handle(SetDefaultCustomerAddressCommand request, CancellationToken cancellationToken)
    {
        var customer = await _repository.GetByIdAsync(request.CustomerId, cancellationToken);
        if (customer is null)
            return new CustomerAddressDto { CustomerId = request.CustomerId, Id = request.AddressId };

        var address = customer.SetDefaultAddress(request.AddressId);
        await _repository.UpdateAsync(customer, cancellationToken);
        return CustomerAddressDto.MapFromEntity(address);
    }
}
