using System.Diagnostics;

namespace Catalog.Domain.Entities;

public class Category
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;

    private Category() { }

    public Category(string name)
    {
        Validate(name);

        Id = Guid.NewGuid();
        Name = (name ?? string.Empty).Trim();
    }

    public void Update(string name)
    {
        Validate(name);
        Name = (name ?? string.Empty).Trim();
    }

    private static void Validate(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            Trace.TraceError("Invalid category name.");
    }
}
