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
    public decimal HeightCm { get; private set; }
    public decimal WidthCm { get; private set; }
    public decimal CubageM3 { get; private set; }
    public decimal WeightKg { get; private set; }
    public string OriginZipCode { get; private set; } = string.Empty;

    private Product() { }

    public Product(
        string name,
        string description,
        decimal price,
        int stockQuantity,
        Guid categoryId,
        decimal heightCm,
        decimal widthCm,
        decimal cubageM3,
        decimal weightKg,
        string originZipCode)
    {
        Validate(name, price, stockQuantity, categoryId, heightCm, widthCm, cubageM3, weightKg, originZipCode);

        Id = Guid.NewGuid();
        Name = name.Trim();
        Description = description?.Trim() ?? string.Empty;
        Price = price;
        StockQuantity = stockQuantity;
        CategoryId = categoryId;
        HeightCm = heightCm;
        WidthCm = widthCm;
        CubageM3 = cubageM3;
        WeightKg = weightKg;
        OriginZipCode = originZipCode.Trim();
        Active = true;
    }

    public void Update(
        string name,
        string description,
        decimal price,
        int stockQuantity,
        Guid categoryId,
        decimal heightCm,
        decimal widthCm,
        decimal cubageM3,
        decimal weightKg,
        string originZipCode)
    {
        Validate(name, price, stockQuantity, categoryId, heightCm, widthCm, cubageM3, weightKg, originZipCode);

        Name = name.Trim();
        Description = description?.Trim() ?? string.Empty;
        Price = price;
        StockQuantity = stockQuantity;
        CategoryId = categoryId;
        HeightCm = heightCm;
        WidthCm = widthCm;
        CubageM3 = cubageM3;
        WeightKg = weightKg;
        OriginZipCode = originZipCode.Trim();
    }

    public void IncreaseStock(int quantity)
    {
        if (quantity <= 0)
            throw new Catalog.Domain.Exceptions.InvalidStockQuantityException();

        StockQuantity += quantity;
    }

    public bool MatchesCatalogDefinition(
        string name,
        string description,
        decimal price,
        Guid categoryId,
        decimal heightCm,
        decimal widthCm,
        decimal cubageM3,
        decimal weightKg,
        string originZipCode)
    {
        return string.Equals(Name, name.Trim(), StringComparison.OrdinalIgnoreCase)
            && string.Equals(Description, description?.Trim() ?? string.Empty, StringComparison.OrdinalIgnoreCase)
            && Price == price
            && CategoryId == categoryId
            && HeightCm == heightCm
            && WidthCm == widthCm
            && CubageM3 == cubageM3
            && WeightKg == weightKg
            && string.Equals(OriginZipCode, originZipCode.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    public void Deactivate() => Active = false;
    public void Activate() => Active = true;
    private static void Validate(
        string name,
        decimal price,
        int stockQuantity,
        Guid categoryId,
        decimal heightCm,
        decimal widthCm,
        decimal cubageM3,
        decimal weightKg,
        string originZipCode)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new Catalog.Domain.Exceptions.InvalidProductNameException();

        if (price <= 0)
            throw new Catalog.Domain.Exceptions.InvalidProductPriceException();

        if (stockQuantity <= 0)
            throw new Catalog.Domain.Exceptions.InvalidStockQuantityException();

        if (categoryId == Guid.Empty)
            throw new Catalog.Domain.Exceptions.InvalidCategoryIdException();

        if (heightCm <= 0)
            throw new Catalog.Domain.Exceptions.InvalidProductHeightException();

        if (widthCm <= 0)
            throw new Catalog.Domain.Exceptions.InvalidProductWidthException();

        if (cubageM3 <= 0)
            throw new Catalog.Domain.Exceptions.InvalidProductCubageException();

        if (weightKg <= 0)
            throw new Catalog.Domain.Exceptions.InvalidProductWeightException();

        if (string.IsNullOrWhiteSpace(originZipCode))
            throw new Catalog.Domain.Exceptions.InvalidProductOriginZipCodeException();
    }
}
