namespace Catalog.Application.ReadModels;

public class ProductReadModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public bool Active { get; set; }
    public Guid CategoryId { get; set; }
}
