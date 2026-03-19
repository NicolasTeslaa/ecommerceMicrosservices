using Customer.Application.DTOs;
using MediatR;

namespace Customer.Application.Queries;

public class GetCustomerByIdQuery : IRequest<CustomerDto>
{
    public GetCustomerByIdQuery(Guid id) => Id = id;

    public Guid Id { get; }
}
