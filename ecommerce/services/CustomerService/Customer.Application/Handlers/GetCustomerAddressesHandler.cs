using Customer.Application.DTOs;
using Customer.Application.Interfaces;
using Customer.Application.Queries;
using MediatR;

namespace Customer.Application.Handlers;

public class GetCustomerAddressesHandler : IRequestHandler<GetCustomerAddressesQuery, IReadOnlyCollection<CustomerAddressDto>>
{
    private readonly ICustomerRepository _repository;

    public GetCustomerAddressesHandler(ICustomerRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyCollection<CustomerAddressDto>> Handle(GetCustomerAddressesQuery request, CancellationToken cancellationToken)
    {
        var customer = await _repository.GetByIdAsync(request.CustomerId, cancellationToken);

        return customer is null
            ? Array.Empty<CustomerAddressDto>()
            : customer.Addresses.Select(CustomerAddressDto.MapFromEntity).ToArray();
    }
}
