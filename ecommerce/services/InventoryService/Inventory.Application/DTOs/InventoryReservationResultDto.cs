namespace Inventory.Application.DTOs;

public class InventoryReservationResultDto
{
    public bool IsSuccess { get; set; }
    public string Reason { get; set; } = string.Empty;
    public IReadOnlyCollection<InventoryReservationIssueDto> Issues { get; set; } = Array.Empty<InventoryReservationIssueDto>();
}
