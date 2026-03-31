using NotaFiscal.Infrastructure.Messaging;
using NotaFiscal.Tests.Support;

namespace NotaFiscal.Tests.Infrastructure;

public class MockInvoiceFactoryTests
{
    [Fact]
    public void Create_ShouldGenerateIssuedInvoice_WithFakeXmlAndAccessKey()
    {
        var factory = new MockInvoiceFactory();
        var integrationEvent = NotaFiscalTestData.CreateOrderConfirmedEvent();

        var invoice = factory.Create(integrationEvent);

        Assert.Equal(integrationEvent.OrderId, invoice.OrderId);
        Assert.Equal("1", invoice.Series);
        Assert.Equal(44, invoice.AccessKey.Length);
        Assert.Contains("<MockNFe>", invoice.XmlContent);
        Assert.Contains("Notebook Gamer", invoice.XmlContent);
        Assert.Equal("brl", invoice.Currency);
    }
}
