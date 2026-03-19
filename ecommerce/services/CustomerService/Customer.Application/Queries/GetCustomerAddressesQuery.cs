using Customer.Application.DTOs;
using MediatR;

namespace Customer.Application.Queries;

public record GetCustomerAddressesQuery(Guid CustomerId) : IRequest<IReadOnlyCollection<CustomerAddressDto>>;
