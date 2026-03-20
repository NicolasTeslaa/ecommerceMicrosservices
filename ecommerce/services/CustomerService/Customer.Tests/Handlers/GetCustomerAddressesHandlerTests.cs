using Customer.Application.Handlers;
using Customer.Application.Interfaces;
using Customer.Application.Queries;
using Customer.Domain.Exceptions;
using Customer.Tests.Support;
using Moq;

namespace Customer.Tests.Handlers;

public class GetCustomerAddressesHandlerTests
{
    private readonly Mock<ICustomerRepository> _repositoryMock = new();
    private readonly GetCustomerAddressesHandler _handler;

    public GetCustomerAddressesHandlerTests()
    {
        _handler = new GetCustomerAddressesHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnAddresses_WhenCustomerExists()
    {
        var customer = CustomerTestData.CreateCustomer();
        CustomerTestData.AddAddress(customer, true);
        CustomerTestData.AddAddress(customer, false);
        _repositoryMock.Setup(repository => repository.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>())).ReturnsAsync(customer);

        var result = await _handler.Handle(new GetCustomerAddressesQuery(customer.Id), CancellationToken.None);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task Handle_ShouldThrowCustomerNotFoundException_WhenCustomerDoesNotExist()
    {
        var customerId = Guid.NewGuid();
        _repositoryMock.Setup(repository => repository.GetByIdAsync(customerId, It.IsAny<CancellationToken>())).ReturnsAsync((Customer.Domain.Entities.Customer?)null);

        var act = () => _handler.Handle(new GetCustomerAddressesQuery(customerId), CancellationToken.None);

        await Assert.ThrowsAsync<CustomerNotFoundException>(act);
    }
}
