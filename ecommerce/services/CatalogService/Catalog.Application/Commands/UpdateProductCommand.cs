namespace Catalog.Application.Commands;

public class UpdateProductCommand : MediatR.IRequest<Guid>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public Guid CategoryId { get; set; }
    public decimal HeightCm { get; set; }
    public decimal WidthCm { get; set; }
    public decimal CubageM3 { get; set; }
    public decimal WeightKg { get; set; }
    public string OriginZipCode { get; set; } = string.Empty;
}
