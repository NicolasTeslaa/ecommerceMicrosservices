using MediatR;

namespace Customer.Application.Commands;

public record RemoveCustomerAddressCommand(Guid CustomerId, Guid AddressId) : IRequest<Unit>;
