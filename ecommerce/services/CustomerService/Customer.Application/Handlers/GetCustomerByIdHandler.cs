using Customer.Application.DTOs;
using Customer.Application.Interfaces;
using Customer.Application.Queries;
using MediatR;

namespace Customer.Application.Handlers;

public class GetCustomerByIdHandler : IRequestHandler<GetCustomerByIdQuery, CustomerDto>
{
    private readonly ICustomerRepository _repository;

    public GetCustomerByIdHandler(ICustomerRepository repository) => _repository = repository;

    public async Task<CustomerDto> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        var customer = await _repository.GetByIdAsync(request.Id, cancellationToken);

        return customer is null
            ? new CustomerDto { Id = request.Id }
            : CustomerDto.MapFromEntity(customer);
    }
}
