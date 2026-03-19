namespace Order.Application.DTOs;

public class OrderProcessingAcceptedDto
{
    public Guid OrderId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime RequestedAtUtc { get; set; }
}
