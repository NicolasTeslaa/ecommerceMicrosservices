using Customer.Application.Commands;
using Customer.Application.Handlers;
using Customer.Application.Interfaces;
using Customer.Domain.Exceptions;
using Customer.Tests.Support;
using MediatR;
using Moq;

namespace Customer.Tests.Handlers;

public class RemoveCustomerAddressHandlerTests
{
    private readonly Mock<ICustomerRepository> _repositoryMock = new();
    private readonly RemoveCustomerAddressHandler _handler;

    public RemoveCustomerAddressHandlerTests()
    {
        _handler = new RemoveCustomerAddressHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldRemoveAddress_WhenCustomerAndAddressExist()
    {
        var customer = CustomerTestData.CreateCustomer();
        var address = CustomerTestData.AddAddress(customer, true);
        _repositoryMock.Setup(repository => repository.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>())).ReturnsAsync(customer);

        var result = await _handler.Handle(new RemoveCustomerAddressCommand(customer.Id, address.Id), CancellationToken.None);

        Assert.Equal(Unit.Value, result);
        Assert.Empty(customer.Addresses);
        _repositoryMock.Verify(repository => repository.UpdateAsync(customer, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowCustomerNotFoundException_WhenCustomerDoesNotExist()
    {
        var command = new RemoveCustomerAddressCommand(Guid.NewGuid(), Guid.NewGuid());
        _repositoryMock.Setup(repository => repository.GetByIdAsync(command.CustomerId, It.IsAny<CancellationToken>())).ReturnsAsync((Customer.Domain.Entities.Customer?)null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await Assert.ThrowsAsync<CustomerNotFoundException>(act);
    }

    [Fact]
    public async Task Handle_ShouldThrowCustomerAddressNotFoundException_WhenAddressDoesNotExist()
    {
        var customer = CustomerTestData.CreateCustomer();
        _repositoryMock.Setup(repository => repository.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>())).ReturnsAsync(customer);

        var act = () => _handler.Handle(new RemoveCustomerAddressCommand(customer.Id, Guid.NewGuid()), CancellationToken.None);

        await Assert.ThrowsAsync<CustomerAddressNotFoundException>(act);
    }
}
