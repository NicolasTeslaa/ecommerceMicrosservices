using Microsoft.AspNetCore.Mvc;
using Payment.Application.Interfaces;

namespace Payment.API.Controllers;

[ApiController]
[Route("api/payments/webhooks/stripe")]
public class StripeWebhooksController : ControllerBase
{
    private readonly IStripeWebhookHandler _webhookHandler;

    public StripeWebhooksController(IStripeWebhookHandler webhookHandler)
    {
        _webhookHandler = webhookHandler;
    }

    [HttpPost]
    public async Task<IActionResult> Handle(CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(HttpContext.Request.Body);
        var json = await reader.ReadToEndAsync(cancellationToken);

        var signatureHeader = Request.Headers["Stripe-Signature"].ToString();
        await _webhookHandler.HandleAsync(json, signatureHeader, cancellationToken);
        return Ok();
    }
}
