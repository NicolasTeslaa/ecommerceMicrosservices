using Customer.Application.Handlers;
using Customer.Application.Interfaces;
using Customer.Domain.Exceptions;
using Customer.Tests.Support;
using Moq;

namespace Customer.Tests.Handlers;

public class UpsertCustomerAddressHandlerTests
{
    private readonly Mock<ICustomerRepository> _repositoryMock = new();
    private readonly UpsertCustomerAddressHandler _handler;

    public UpsertCustomerAddressHandlerTests()
    {
        _handler = new UpsertCustomerAddressHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldAddAddress_WhenAddressIdIsNull()
    {
        var customer = CustomerTestData.CreateCustomer();
        var command = CustomerTestData.CreateUpsertCommand(customer.Id, null, true);
        _repositoryMock.Setup(repository => repository.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>())).ReturnsAsync(customer);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.Equal("Casa", result.Label);
        Assert.Single(customer.Addresses);
        _repositoryMock.Verify(repository => repository.UpdateAsync(customer, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldUpdateAddress_WhenAddressIdHasValue()
    {
        var customer = CustomerTestData.CreateCustomer();
        var address = CustomerTestData.AddAddress(customer, true);
        var command = CustomerTestData.CreateUpsertCommand(customer.Id, address.Id, false);
        command.Label = "Trabalho";
        _repositoryMock.Setup(repository => repository.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>())).ReturnsAsync(customer);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.Equal(address.Id, result.Id);
        Assert.Equal("Trabalho", result.Label);
    }

    [Fact]
    public async Task Handle_ShouldTreatEmptyAddressIdAsAddOperation()
    {
        var customer = CustomerTestData.CreateCustomer();
        var command = CustomerTestData.CreateUpsertCommand(customer.Id, Guid.Empty, false);
        _repositoryMock.Setup(repository => repository.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>())).ReturnsAsync(customer);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.Single(customer.Addresses);
        Assert.Equal("Casa", result.Label);
    }

    [Fact]
    public async Task Handle_ShouldThrowCustomerNotFoundException_WhenCustomerDoesNotExist()
    {
        var command = CustomerTestData.CreateUpsertCommand();
        _repositoryMock.Setup(repository => repository.GetByIdAsync(command.CustomerId, It.IsAny<CancellationToken>())).ReturnsAsync((Customer.Domain.Entities.Customer?)null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await Assert.ThrowsAsync<CustomerNotFoundException>(act);
    }

    [Fact]
    public async Task Handle_ShouldBubbleValidationException_WhenAddressDataIsInvalid()
    {
        var customer = CustomerTestData.CreateCustomer();
        var command = CustomerTestData.CreateUpsertCommand(customer.Id, null, false);
        command.City = string.Empty;
        _repositoryMock.Setup(repository => repository.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>())).ReturnsAsync(customer);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await Assert.ThrowsAsync<InvalidCityException>(act);
    }
}
