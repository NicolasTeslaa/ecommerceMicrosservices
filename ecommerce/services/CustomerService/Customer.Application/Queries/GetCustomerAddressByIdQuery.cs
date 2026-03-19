using Customer.Application.DTOs;
using MediatR;

namespace Customer.Application.Queries;

public record GetCustomerAddressByIdQuery(Guid CustomerId, Guid AddressId) : IRequest<CustomerAddressDto>;
