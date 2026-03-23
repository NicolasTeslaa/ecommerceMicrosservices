using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Payment.API.Controllers;
using Payment.Application.Interfaces;

namespace Payment.Tests.Controllers;

public class StripeWebhooksControllerTests
{
    [Fact]
    public async Task Handle_ShouldForwardPayloadAndSignatureHeader()
    {
        var webhookHandler = new Mock<IStripeWebhookHandler>();
        var controller = new StripeWebhooksController(webhookHandler.Object);
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes("{\"id\":\"evt_1\"}"));
        httpContext.Request.Headers["Stripe-Signature"] = "sig_123";
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        var result = await controller.Handle(CancellationToken.None);

        Assert.IsType<OkResult>(result);
        webhookHandler.Verify(item => item.HandleAsync("{\"id\":\"evt_1\"}", "sig_123", It.IsAny<CancellationToken>()), Times.Once);
    }
}
