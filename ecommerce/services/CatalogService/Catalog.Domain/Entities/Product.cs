namespace Catalog.Domain.Entities;

public class Product
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public int StockQuantity { get; private set; }
    public bool Active { get; private set; }
    public Guid CategoryId { get; private set; }

    private Product() { }

    public Product(string name, string description, decimal price, int stockQuantity, Guid categoryId)
    {
        Validate(name, price, stockQuantity, categoryId);

        Id = Guid.NewGuid();
        Name = name.Trim();
        Description = description?.Trim() ?? string.Empty;
        Price = price;
        StockQuantity = stockQuantity;
        CategoryId = categoryId;
        Active = true;
    }

    public void Update(string name, string description, decimal price, int stockQuantity, Guid categoryId)
    {
        Validate(name, price, stockQuantity, categoryId);

        Name = name.Trim();
        Description = description?.Trim() ?? string.Empty;
        Price = price;
        StockQuantity = stockQuantity;
        CategoryId = categoryId;
    }

    public void Deactivate() => Active = false;
    public void Activate() => Active = true;
    private static void Validate(string name, decimal price, int stockQuantity, Guid categoryId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new Catalog.Domain.Exceptions.InvalidProductNameException();

        if (price <= 0)
            throw new Catalog.Domain.Exceptions.InvalidProductPriceException();

        if (stockQuantity <= 0)
            throw new Catalog.Domain.Exceptions.InvalidStockQuantityException();

        if (categoryId == Guid.Empty)
            throw new Catalog.Domain.Exceptions.InvalidCategoryIdException();
    }
}
