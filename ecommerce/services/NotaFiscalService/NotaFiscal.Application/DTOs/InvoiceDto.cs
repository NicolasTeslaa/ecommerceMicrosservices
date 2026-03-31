namespace NotaFiscal.Application.DTOs;

public class InvoiceDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public long Number { get; set; }
    public string Series { get; set; } = string.Empty;
    public string AccessKey { get; set; } = string.Empty;
    public string XmlContent { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime IssuedAtUtc { get; set; }
}
