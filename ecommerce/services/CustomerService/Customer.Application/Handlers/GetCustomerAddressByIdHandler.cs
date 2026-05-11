using Customer.Application.DTOs;
using Customer.Application.Interfaces;
using Customer.Application.Queries;
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
        var customer = await _repository.GetByIdAsync(request.CustomerId, cancellationToken);

        if (customer is null)
            return new CustomerAddressDto { CustomerId = request.CustomerId, Id = request.AddressId };

        return CustomerAddressDto.MapFromEntity(customer.GetAddress(request.AddressId));
    }
}
