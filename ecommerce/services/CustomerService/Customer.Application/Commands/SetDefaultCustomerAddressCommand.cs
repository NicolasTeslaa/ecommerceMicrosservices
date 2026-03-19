using Customer.Application.DTOs;
using MediatR;

namespace Customer.Application.Commands;

public record SetDefaultCustomerAddressCommand(Guid CustomerId, Guid AddressId) : IRequest<CustomerAddressDto>;
