using NotaFiscal.Domain.Enums;

namespace NotaFiscal.Domain.Entities;

public class Invoice
{
    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public Guid CustomerId { get; private set; }
    public long Number { get; private set; }
    public string Series { get; private set; } = string.Empty;
    public string AccessKey { get; private set; } = string.Empty;
    public string XmlContent { get; private set; } = string.Empty;
    public InvoiceStatus Status { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public DateTime IssuedAtUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    private Invoice()
    {
    }

    public Invoice(
        Guid orderId,
        Guid customerId,
        long number,
        string series,
        string accessKey,
        string xmlContent,
        decimal totalAmount,
        string currency,
        DateTime issuedAtUtc)
    {
        if (orderId == Guid.Empty)
            throw new InvalidOperationException("OrderId must be provided.");
        if (customerId == Guid.Empty)
            throw new InvalidOperationException("CustomerId must be provided.");
        if (number <= 0)
            throw new InvalidOperationException("Invoice number must be greater than zero.");
        if (string.IsNullOrWhiteSpace(series))
            throw new InvalidOperationException("Invoice series must be provided.");
        if (string.IsNullOrWhiteSpace(accessKey))
            throw new InvalidOperationException("Invoice access key must be provided.");
        if (string.IsNullOrWhiteSpace(xmlContent))
            throw new InvalidOperationException("Invoice XML content must be provided.");
        if (totalAmount <= 0)
            throw new InvalidOperationException("Invoice total amount must be greater than zero.");
        if (string.IsNullOrWhiteSpace(currency))
            throw new InvalidOperationException("Invoice currency must be provided.");

        Id = Guid.NewGuid();
        OrderId = orderId;
        CustomerId = customerId;
        Number = number;
        Series = series.Trim();
        AccessKey = accessKey.Trim();
        XmlContent = xmlContent.Trim();
        Status = InvoiceStatus.Issued;
        TotalAmount = totalAmount;
        Currency = currency.Trim().ToLowerInvariant();
        IssuedAtUtc = issuedAtUtc;
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = CreatedAtUtc;
    }
}
