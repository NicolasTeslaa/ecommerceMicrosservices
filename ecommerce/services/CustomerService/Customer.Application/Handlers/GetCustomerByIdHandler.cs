using Customer.Application.DTOs;
using Customer.Application.Interfaces;
using Customer.Application.Queries;
using Customer.Domain.Exceptions;
using MediatR;

namespace Customer.Application.Handlers;

public class GetCustomerByIdHandler : IRequestHandler<GetCustomerByIdQuery, CustomerDto>
{
    private readonly ICustomerRepository _repository;

    public GetCustomerByIdHandler(ICustomerRepository repository) => _repository = repository;

    public async Task<CustomerDto> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        var customer = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new CustomerNotFoundException(request.Id);

        return CustomerDto.MapFromEntity(customer);
    }
}
