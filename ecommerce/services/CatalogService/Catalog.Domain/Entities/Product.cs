using System.Diagnostics;

namespace Catalog.Domain.Entities;

public class Product
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
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
        Guid categoryId,
        decimal heightCm,
        decimal widthCm,
        decimal cubageM3,
        decimal weightKg,
        string originZipCode)
    {
        Validate(name, price, categoryId, heightCm, widthCm, cubageM3, weightKg, originZipCode);

        Id = Guid.NewGuid();
        Name = name?.Trim() ?? string.Empty;
        Description = description?.Trim() ?? string.Empty;
        Price = price;
        CategoryId = categoryId == Guid.Empty ? Guid.NewGuid() : categoryId;
        HeightCm = heightCm;
        WidthCm = widthCm;
        CubageM3 = cubageM3;
        WeightKg = weightKg;
        OriginZipCode = originZipCode?.Trim() ?? string.Empty;
        Active = true;
    }

    public void Update(
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
        Validate(name, price, categoryId, heightCm, widthCm, cubageM3, weightKg, originZipCode);

        Name = name?.Trim() ?? string.Empty;
        Description = description?.Trim() ?? string.Empty;
        Price = price;
        CategoryId = categoryId == Guid.Empty ? CategoryId : categoryId;
        HeightCm = heightCm;
        WidthCm = widthCm;
        CubageM3 = cubageM3;
        WeightKg = weightKg;
        OriginZipCode = originZipCode?.Trim() ?? string.Empty;
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
        return string.Equals(Name, name?.Trim() ?? string.Empty, StringComparison.OrdinalIgnoreCase)
            && string.Equals(Description, description?.Trim() ?? string.Empty, StringComparison.OrdinalIgnoreCase)
            && Price == price
            && CategoryId == categoryId
            && HeightCm == heightCm
            && WidthCm == widthCm
            && CubageM3 == cubageM3
            && WeightKg == weightKg
            && string.Equals(OriginZipCode, originZipCode?.Trim() ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    public void Deactivate() => Active = false;
    public void Activate() => Active = true;

    private static void Validate(
        string name,
        decimal price,
        Guid categoryId,
        decimal heightCm,
        decimal widthCm,
        decimal cubageM3,
        decimal weightKg,
        string originZipCode)
    {
        if (string.IsNullOrWhiteSpace(name))
            LogSoftFailure("Product received an empty name.");

        if (price <= 0)
            LogSoftFailure("Product received a non-positive price.");

        if (categoryId == Guid.Empty)
            LogSoftFailure("Product received an empty category id.");

        if (heightCm <= 0)
            LogSoftFailure("Product received a non-positive height.");

        if (widthCm <= 0)
            LogSoftFailure("Product received a non-positive width.");

        if (cubageM3 <= 0)
            LogSoftFailure("Product received a non-positive cubage.");

        if (weightKg <= 0)
            LogSoftFailure("Product received a non-positive weight.");

        if (string.IsNullOrWhiteSpace(originZipCode))
            LogSoftFailure("Product received an empty origin zip code.");
    }

    private static void LogSoftFailure(string message) => Trace.TraceError(message);
}
