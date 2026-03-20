using Customer.API.Controllers;
using Customer.Application.Commands;
using Customer.Application.DTOs;
using Customer.Application.Queries;
using ECommerce.Shared.Contracts;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Customer.Tests.Controllers;

public class CustomersControllerTests
{
    private readonly Mock<IMediator> _mediatorMock = new();
    private readonly CustomersController _controller;

    public CustomersControllerTests()
    {
        _controller = new CustomersController(_mediatorMock.Object);
    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenMediatorReturnsCustomer()
    {
        var customerId = Guid.NewGuid();
        _mediatorMock.Setup(mediator => mediator.Send(It.IsAny<GetCustomerByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CustomerDto { Id = customerId, FullName = "Jane Doe" });

        var result = await _controller.GetById(customerId);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<CustomerDto>>(ok.Value);
        Assert.True(response.Success);
        Assert.Equal(customerId, response.Data!.Id);
    }

    [Fact]
    public async Task GetAddresses_ShouldReturnOk_WhenMediatorReturnsAddresses()
    {
        var customerId = Guid.NewGuid();
        _mediatorMock.Setup(mediator => mediator.Send(It.IsAny<GetCustomerAddressesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new CustomerAddressDto { Id = Guid.NewGuid(), CustomerId = customerId }]);

        var result = await _controller.GetAddresses(customerId);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<IReadOnlyCollection<CustomerAddressDto>>>(ok.Value);
        Assert.Single(response.Data!);
    }

    [Fact]
    public async Task GetAddressById_ShouldReturnOk_WhenMediatorReturnsAddress()
    {
        var customerId = Guid.NewGuid();
        var addressId = Guid.NewGuid();
        _mediatorMock.Setup(mediator => mediator.Send(It.IsAny<GetCustomerAddressByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CustomerAddressDto { Id = addressId, CustomerId = customerId });

        var result = await _controller.GetAddressById(customerId, addressId);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<CustomerAddressDto>>(ok.Value);
        Assert.Equal(addressId, response.Data!.Id);
    }

    [Fact]
    public async Task AddAddress_ShouldInjectRouteCustomerId_AndClearAddressId()
    {
        var customerId = Guid.NewGuid();
        var command = new UpsertCustomerAddressCommand { AddressId = Guid.NewGuid(), Label = "Casa", RecipientName = "Jane", Street = "Rua A", Number = "1", Neighborhood = "Centro", City = "Sao Paulo", State = "SP", ZipCode = "01000-000", Country = "Brasil" };
        _mediatorMock.Setup(mediator => mediator.Send(It.IsAny<UpsertCustomerAddressCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CustomerAddressDto { CustomerId = customerId });

        await _controller.AddAddress(customerId, command);

        _mediatorMock.Verify(
            mediator => mediator.Send(
                It.Is<UpsertCustomerAddressCommand>(item => item.CustomerId == customerId && item.AddressId == null),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAddress_ShouldInjectRouteIdsIntoCommand()
    {
        var customerId = Guid.NewGuid();
        var addressId = Guid.NewGuid();
        var command = new UpsertCustomerAddressCommand { Label = "Casa", RecipientName = "Jane", Street = "Rua A", Number = "1", Neighborhood = "Centro", City = "Sao Paulo", State = "SP", ZipCode = "01000-000", Country = "Brasil" };
        _mediatorMock.Setup(mediator => mediator.Send(It.IsAny<UpsertCustomerAddressCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CustomerAddressDto { Id = addressId, CustomerId = customerId });

        await _controller.UpdateAddress(customerId, addressId, command);

        _mediatorMock.Verify(
            mediator => mediator.Send(
                It.Is<UpsertCustomerAddressCommand>(item => item.CustomerId == customerId && item.AddressId == addressId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SetDefaultAddress_ShouldDispatchSetDefaultCommand()
    {
        var customerId = Guid.NewGuid();
        var addressId = Guid.NewGuid();
        _mediatorMock.Setup(mediator => mediator.Send(It.IsAny<SetDefaultCustomerAddressCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CustomerAddressDto { Id = addressId, CustomerId = customerId, IsDefault = true });

        var result = await _controller.SetDefaultAddress(customerId, addressId);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<CustomerAddressDto>>(ok.Value);
        Assert.True(response.Data!.IsDefault);
    }

    [Fact]
    public async Task RemoveAddress_ShouldReturnSuccessResponse_WhenMediatorCompletes()
    {
        var customerId = Guid.NewGuid();
        var addressId = Guid.NewGuid();
        _mediatorMock.Setup(mediator => mediator.Send(It.IsAny<RemoveCustomerAddressCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Value);

        var result = await _controller.RemoveAddress(customerId, addressId);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<object?>>(ok.Value);
        Assert.True(response.Success);
        Assert.Null(response.Data);
    }
}
