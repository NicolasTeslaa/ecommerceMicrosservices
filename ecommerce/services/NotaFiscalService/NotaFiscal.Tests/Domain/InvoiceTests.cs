using NotaFiscal.Domain.Entities;
using NotaFiscal.Domain.Enums;

namespace NotaFiscal.Tests.Domain;

public class InvoiceTests
{
    [Fact]
    public void Constructor_ShouldCreateIssuedInvoice_WithNormalizedCurrency()
    {
        var invoice = new Invoice(
            Guid.NewGuid(),
            Guid.NewGuid(),
            123,
            "A1",
            "12345678901234567890123456789012345678901234",
            "<xml />",
            99.9m,
            "BRL",
            DateTime.UtcNow);

        Assert.Equal(InvoiceStatus.Issued, invoice.Status);
        Assert.Equal("brl", invoice.Currency);
        Assert.Equal("A1", invoice.Series);
        Assert.False(invoice.CreatedAtUtc == default);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenOrderIdIsEmpty()
    {
        Assert.Throws<InvalidOperationException>(() => new Invoice(
            Guid.Empty,
            Guid.NewGuid(),
            123,
            "1",
            "12345678901234567890123456789012345678901234",
            "<xml />",
            120m,
            "brl",
            DateTime.UtcNow));
    }
}
