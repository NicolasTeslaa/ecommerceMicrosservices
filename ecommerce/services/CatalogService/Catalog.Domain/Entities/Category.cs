using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        Name = name.Trim();
    }

    public void Update(string name)
    {
        Validate(name);

        Name = name.Trim();
    }

    private static void Validate(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new Catalog.Domain.Exceptions.InvalidCategoryNameException();
    }
}
