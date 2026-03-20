namespace Order.Application.DTOs;

public class ProductAvailabilityValidationResultDto
{
    public bool IsValid { get; set; }
    public string Reason { get; set; } = string.Empty;
    public IReadOnlyCollection<ProductAvailabilityIssueDto> Issues { get; set; } = Array.Empty<ProductAvailabilityIssueDto>();
}
