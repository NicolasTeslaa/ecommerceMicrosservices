using Customer.Application.Handlers;
using Customer.Application.Interfaces;
using Customer.Application.Queries;
using Customer.Domain.Exceptions;
using Customer.Tests.Support;
using Moq;

namespace Customer.Tests.Handlers;

public class GetCustomerByIdHandlerTests
{
    private readonly Mock<ICustomerRepository> _repositoryMock = new();
    private readonly GetCustomerByIdHandler _handler;

    public GetCustomerByIdHandlerTests()
    {
        _handler = new GetCustomerByIdHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnCustomerDto_WhenCustomerExists()
    {
        var customer = CustomerTestData.CreateCustomer();
        _repositoryMock.Setup(repository => repository.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>())).ReturnsAsync(customer);

        var result = await _handler.Handle(new GetCustomerByIdQuery(customer.Id), CancellationToken.None);

        Assert.Equal(customer.Id, result.Id);
        Assert.Equal(customer.Email, result.Email);
    }

    [Fact]
    public async Task Handle_ShouldThrowCustomerNotFoundException_WhenCustomerDoesNotExist()
    {
        var customerId = Guid.NewGuid();
        _repositoryMock.Setup(repository => repository.GetByIdAsync(customerId, It.IsAny<CancellationToken>())).ReturnsAsync((Customer.Domain.Entities.Customer?)null);

        var act = () => _handler.Handle(new GetCustomerByIdQuery(customerId), CancellationToken.None);

        await Assert.ThrowsAsync<CustomerNotFoundException>(act);
    }
}
