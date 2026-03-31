namespace ECommerce.Shared.Messaging;

public class InvoiceIssuedIntegrationEvent
{
    public Guid InvoiceId { get; set; }
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public long Number { get; set; }
    public string Series { get; set; } = string.Empty;
    public string AccessKey { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime IssuedAtUtc { get; set; }
}
