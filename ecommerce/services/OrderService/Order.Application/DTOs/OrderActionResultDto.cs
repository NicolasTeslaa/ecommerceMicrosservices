namespace Order.Application.DTOs;

public class OrderActionResultDto
{
    public Guid OrderId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
