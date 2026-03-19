using Customer.Application.Commands;
using Customer.Application.DTOs;
using Customer.Application.Queries;
using ECommerce.Shared.Contracts;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Customer.API.Controllers;

[ApiController]
[Route("api/customers")]
public class CustomersController : ControllerBase
{
    private readonly IMediator _mediator;

    public CustomersController(IMediator mediator) => _mediator = mediator;

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<CustomerDto>>> GetById(Guid id)
    {
        var customer = await _mediator.Send(new GetCustomerByIdQuery(id));
        return Ok(ApiResponse<CustomerDto>.Ok(customer, "Customer retrieved successfully."));
    }

    [HttpGet("{id:guid}/addresses")]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<CustomerAddressDto>>>> GetAddresses(Guid id)
    {
        var addresses = await _mediator.Send(new GetCustomerAddressesQuery(id));
        return Ok(ApiResponse<IReadOnlyCollection<CustomerAddressDto>>.Ok(addresses, "Customer addresses retrieved successfully."));
    }

    [HttpGet("{id:guid}/addresses/{addressId:guid}")]
    public async Task<ActionResult<ApiResponse<CustomerAddressDto>>> GetAddressById(Guid id, Guid addressId)
    {
        var address = await _mediator.Send(new GetCustomerAddressByIdQuery(id, addressId));
        return Ok(ApiResponse<CustomerAddressDto>.Ok(address, "Customer address retrieved successfully."));
    }

    [HttpPost("{id:guid}/addresses")]
    public async Task<ActionResult<ApiResponse<CustomerAddressDto>>> AddAddress(Guid id, [FromBody] UpsertCustomerAddressCommand command)
    {
        command.CustomerId = id;
        command.AddressId = null;
        var address = await _mediator.Send(command);
        return Ok(ApiResponse<CustomerAddressDto>.Ok(address, "Customer address created successfully."));
    }

    [HttpPut("{id:guid}/addresses/{addressId:guid}")]
    public async Task<ActionResult<ApiResponse<CustomerAddressDto>>> UpdateAddress(Guid id, Guid addressId, [FromBody] UpsertCustomerAddressCommand command)
    {
        command.CustomerId = id;
        command.AddressId = addressId;
        var address = await _mediator.Send(command);
        return Ok(ApiResponse<CustomerAddressDto>.Ok(address, "Customer address updated successfully."));
    }

    [HttpPatch("{id:guid}/addresses/{addressId:guid}/default")]
    public async Task<ActionResult<ApiResponse<CustomerAddressDto>>> SetDefaultAddress(Guid id, Guid addressId)
    {
        var address = await _mediator.Send(new SetDefaultCustomerAddressCommand(id, addressId));
        return Ok(ApiResponse<CustomerAddressDto>.Ok(address, "Customer default address updated successfully."));
    }

    [HttpDelete("{id:guid}/addresses/{addressId:guid}")]
    public async Task<ActionResult<ApiResponse<object?>>> RemoveAddress(Guid id, Guid addressId)
    {
        await _mediator.Send(new RemoveCustomerAddressCommand(id, addressId));
        return Ok(ApiResponse<object?>.Ok(null, "Customer address removed successfully."));
    }
}
