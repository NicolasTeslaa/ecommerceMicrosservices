using Customer.Application.Commands;
using Customer.Application.Handlers;
using Customer.Application.Interfaces;
using Customer.Domain.Exceptions;
using Customer.Tests.Support;
using Moq;

namespace Customer.Tests.Handlers;

public class SetDefaultCustomerAddressHandlerTests
{
    private readonly Mock<ICustomerRepository> _repositoryMock = new();
    private readonly SetDefaultCustomerAddressHandler _handler;

    public SetDefaultCustomerAddressHandlerTests()
    {
        _handler = new SetDefaultCustomerAddressHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldSetRequestedAddressAsDefault()
    {
        var customer = CustomerTestData.CreateCustomer();
        CustomerTestData.AddAddress(customer, true);
        var second = customer.AddAddress("Trabalho", "Jane Doe", "Rua B", "99", "", "Centro", "Sao Paulo", "SP", "01000-000", "Brasil", "", false);
        _repositoryMock.Setup(repository => repository.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>())).ReturnsAsync(customer);

        var result = await _handler.Handle(new SetDefaultCustomerAddressCommand(customer.Id, second.Id), CancellationToken.None);

        Assert.True(result.IsDefault);
        Assert.Equal(second.Id, result.Id);
        _repositoryMock.Verify(repository => repository.UpdateAsync(customer, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowCustomerNotFoundException_WhenCustomerDoesNotExist()
    {
        var command = new SetDefaultCustomerAddressCommand(Guid.NewGuid(), Guid.NewGuid());
        _repositoryMock.Setup(repository => repository.GetByIdAsync(command.CustomerId, It.IsAny<CancellationToken>())).ReturnsAsync((Customer.Domain.Entities.Customer?)null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await Assert.ThrowsAsync<CustomerNotFoundException>(act);
    }

    [Fact]
    public async Task Handle_ShouldThrowCustomerAddressNotFoundException_WhenAddressDoesNotExist()
    {
        var customer = CustomerTestData.CreateCustomer();
        _repositoryMock.Setup(repository => repository.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>())).ReturnsAsync(customer);

        var act = () => _handler.Handle(new SetDefaultCustomerAddressCommand(customer.Id, Guid.NewGuid()), CancellationToken.None);

        await Assert.ThrowsAsync<CustomerAddressNotFoundException>(act);
    }
}
