using Customer.Application.Handlers;
using Customer.Application.Interfaces;
using Customer.Application.Queries;
using Customer.Domain.Exceptions;
using Customer.Tests.Support;
using Moq;

namespace Customer.Tests.Handlers;

public class GetCustomerAddressByIdHandlerTests
{
    private readonly Mock<ICustomerRepository> _repositoryMock = new();
    private readonly GetCustomerAddressByIdHandler _handler;

    public GetCustomerAddressByIdHandlerTests()
    {
        _handler = new GetCustomerAddressByIdHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnAddress_WhenCustomerAndAddressExist()
    {
        var customer = CustomerTestData.CreateCustomer();
        var address = CustomerTestData.AddAddress(customer, true);
        _repositoryMock.Setup(repository => repository.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>())).ReturnsAsync(customer);

        var result = await _handler.Handle(new GetCustomerAddressByIdQuery(customer.Id, address.Id), CancellationToken.None);

        Assert.Equal(address.Id, result.Id);
        Assert.Equal(address.Label, result.Label);
    }

    [Fact]
    public async Task Handle_ShouldThrowCustomerNotFoundException_WhenCustomerDoesNotExist()
    {
        var customerId = Guid.NewGuid();
        _repositoryMock.Setup(repository => repository.GetByIdAsync(customerId, It.IsAny<CancellationToken>())).ReturnsAsync((Customer.Domain.Entities.Customer?)null);

        var act = () => _handler.Handle(new GetCustomerAddressByIdQuery(customerId, Guid.NewGuid()), CancellationToken.None);

        await Assert.ThrowsAsync<CustomerNotFoundException>(act);
    }

    [Fact]
    public async Task Handle_ShouldThrowCustomerAddressNotFoundException_WhenAddressDoesNotExist()
    {
        var customer = CustomerTestData.CreateCustomer();
        _repositoryMock.Setup(repository => repository.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>())).ReturnsAsync(customer);

        var act = () => _handler.Handle(new GetCustomerAddressByIdQuery(customer.Id, Guid.NewGuid()), CancellationToken.None);

        await Assert.ThrowsAsync<CustomerAddressNotFoundException>(act);
    }
}
