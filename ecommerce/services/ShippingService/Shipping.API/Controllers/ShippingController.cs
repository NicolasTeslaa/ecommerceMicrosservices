using ECommerce.Shared.Contracts;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shipping.Application.Commands;
using Shipping.Application.DTOs;

namespace Shipping.API.Controllers;

[ApiController]
[Route("api/shipping")]
public class ShippingController : ControllerBase
{
    private readonly IMediator _mediator;

    public ShippingController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("quotes")]
    public async Task<ActionResult<ApiResponse<ShippingQuoteDto>>> CalculateQuote([FromBody] CalculateShippingCommand command)
    {
        var quote = await _mediator.Send(command);
        return Ok(ApiResponse<ShippingQuoteDto>.Ok(quote, "Shipping quote calculated successfully."));
    }
}
