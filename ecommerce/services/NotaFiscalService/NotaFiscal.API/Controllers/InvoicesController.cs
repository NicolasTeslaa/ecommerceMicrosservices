using ECommerce.Shared.Contracts;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NotaFiscal.Application.DTOs;
using NotaFiscal.Application.Queries;

namespace NotaFiscal.API.Controllers;

[ApiController]
[Route("api/invoices")]
public class InvoicesController : ControllerBase
{
    private readonly IMediator _mediator;

    public InvoicesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("orders/{orderId:guid}")]
    public async Task<ActionResult<ApiResponse<InvoiceDto>>> GetByOrderId(Guid orderId, CancellationToken cancellationToken)
    {
        var invoice = await _mediator.Send(new GetInvoiceByOrderIdQuery(orderId), cancellationToken);

        if (invoice is null)
            return NotFound(ApiResponse<InvoiceDto>.Fail("NotFound", "Invoice was not found for the specified order."));

        return Ok(ApiResponse<InvoiceDto>.Ok(invoice, "Invoice retrieved successfully."));
    }
}
