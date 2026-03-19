using Customer.Application.Commands;
using Customer.Application.Interfaces;
using Customer.Domain.Exceptions;
using MediatR;

namespace Customer.Application.Handlers;

public class RemoveCustomerAddressHandler : IRequestHandler<RemoveCustomerAddressCommand, Unit>
{
    private readonly ICustomerRepository _repository;

    public RemoveCustomerAddressHandler(ICustomerRepository repository)
    {
        _repository = repository;
    }

    public async Task<Unit> Handle(RemoveCustomerAddressCommand request, CancellationToken cancellationToken)
    {
        var customer = await _repository.GetByIdAsync(request.CustomerId, cancellationToken)
            ?? throw new CustomerNotFoundException(request.CustomerId);

        customer.RemoveAddress(request.AddressId);
        await _repository.UpdateAsync(customer, cancellationToken);
        return Unit.Value;
    }
}
