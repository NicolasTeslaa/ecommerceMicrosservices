using Customer.Application.DTOs;
using Customer.Application.Interfaces;
using Customer.Application.Queries;
using Customer.Domain.Exceptions;
using MediatR;

namespace Customer.Application.Handlers;

public class GetCustomerAddressByIdHandler : IRequestHandler<GetCustomerAddressByIdQuery, CustomerAddressDto>
{
    private readonly ICustomerRepository _repository;

    public GetCustomerAddressByIdHandler(ICustomerRepository repository)
    {
        _repository = repository;
    }

    public async Task<CustomerAddressDto> Handle(GetCustomerAddressByIdQuery request, CancellationToken cancellationToken)
    {
        var customer = await _repository.GetByIdAsync(request.CustomerId, cancellationToken)
            ?? throw new CustomerNotFoundException(request.CustomerId);

        return CustomerAddressDto.MapFromEntity(customer.GetAddress(request.AddressId));
    }
}
