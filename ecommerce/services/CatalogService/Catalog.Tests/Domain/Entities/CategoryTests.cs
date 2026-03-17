using Catalog.Domain.Entities;
using Catalog.Domain.Exceptions;

namespace Catalog.Tests.Domain.Entities;

public class CategoryTests
{
    [Fact]
    public void Constructor_ShouldCreateCategory_WhenNameIsValid()
    {
        var category = new Category(" Hardware ");

        Assert.NotEqual(Guid.Empty, category.Id);
        Assert.Equal("Hardware", category.Name);
    }

    [Fact]
    public void Constructor_ShouldThrowInvalidCategoryNameException_WhenNameIsEmpty()
    {
        var act = () => new Category("");

        Assert.Throws<InvalidCategoryNameException>(act);
    }

    [Fact]
    public void Constructor_ShouldThrowInvalidCategoryNameException_WhenNameIsWhiteSpace()
    {
        var act = () => new Category("   ");

        Assert.Throws<InvalidCategoryNameException>(act);
    }

    [Fact]
    public void Update_ShouldUpdateName_WhenNameIsValid()
    {
        var category = new Category("Periféricos");

        category.Update(" Monitores ");

        Assert.Equal("Monitores", category.Name);
    }

    [Fact]
    public void Update_ShouldThrowInvalidCategoryNameException_WhenNameIsInvalid()
    {
        var category = new Category("Periféricos");

        var act = () => category.Update("");

        Assert.Throws<InvalidCategoryNameException>(act);
    }
}
