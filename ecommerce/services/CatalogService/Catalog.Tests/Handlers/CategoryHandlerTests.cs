using Catalog.Application.Commands;
using Catalog.Application.Handlers;
using Catalog.Application.Interfaces;
using Catalog.Application.Queries;
using Catalog.Domain.Entities;
using Catalog.Domain.Exceptions;
using Moq;

namespace Catalog.Tests.Handlers;

public class CreateCategoryHandlerTests
{
    private readonly Mock<ICategoryRepository> _repositoryMock;
    private readonly CreateCategoryHandler _handler;

    public CreateCategoryHandlerTests()
    {
        _repositoryMock = new Mock<ICategoryRepository>();
        _handler = new CreateCategoryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateCategory_AndReturnId()
    {
        Category? capturedCategory = null;

        var command = new CreateCategoryCommand
        {
            Name = "Hardware"
        };

        _repositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()))
            .Callback<Category, CancellationToken>((category, _) => capturedCategory = category)
            .Returns(Task.CompletedTask);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, result);
        Assert.NotNull(capturedCategory);
        Assert.Equal("Hardware", capturedCategory!.Name);
        Assert.Equal(capturedCategory.Id, result);
    }
}

public class GetCategoryByIdHandlerTests
{
    private readonly Mock<ICategoryRepository> _repositoryMock;
    private readonly GetCategoryByIdHandler _handler;

    public GetCategoryByIdHandlerTests()
    {
        _repositoryMock = new Mock<ICategoryRepository>();
        _handler = new GetCategoryByIdHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidCategoryIdException_WhenIdIsEmpty()
    {
        var query = new GetCategoryByIdQuery(Guid.Empty);

        var act = () => _handler.Handle(query, CancellationToken.None);

        await Assert.ThrowsAsync<InvalidCategoryIdException>(act);
    }

    [Fact]
    public async Task Handle_ShouldThrowCategoryNotFoundException_WhenCategoryDoesNotExist()
    {
        var categoryId = Guid.NewGuid();
        var query = new GetCategoryByIdQuery(categoryId);

        _repositoryMock
            .Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        var act = () => _handler.Handle(query, CancellationToken.None);

        await Assert.ThrowsAsync<CategoryNotFoundException>(act);
    }

    [Fact]
    public async Task Handle_ShouldReturnMappedCategory_WhenCategoryExists()
    {
        var category = new Category("Hardware");
        var query = new GetCategoryByIdQuery(category.Id);

        _repositoryMock
            .Setup(x => x.GetByIdAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        var result = await _handler.Handle(query, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(category.Id, result!.Id);
        Assert.Equal(category.Name, result.Name);
    }
}

public class GetAllCategoriesHandlerTests
{
    private readonly Mock<ICategoryRepository> _repositoryMock;
    private readonly GetAllCategoriesHandler _handler;

    public GetAllCategoriesHandlerTests()
    {
        _repositoryMock = new Mock<ICategoryRepository>();
        _handler = new GetAllCategoriesHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnMappedCategories()
    {
        var categories = new List<Category>
        {
            new("Hardware"),
            new("Monitores")
        };

        _repositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        var result = await _handler.Handle(new GetAllCategoriesQuery(), CancellationToken.None);
        var list = result.ToList();

        Assert.Equal(2, list.Count);
        Assert.Equal(categories[0].Id, list[0].Id);
        Assert.Equal(categories[0].Name, list[0].Name);
        Assert.Equal(categories[1].Id, list[1].Id);
        Assert.Equal(categories[1].Name, list[1].Name);
    }
}

public class UpdateCategoryHandlerTests
{
    private readonly Mock<ICategoryRepository> _repositoryMock;
    private readonly UpdateCategoryHandler _handler;

    public UpdateCategoryHandlerTests()
    {
        _repositoryMock = new Mock<ICategoryRepository>();
        _handler = new UpdateCategoryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidCategoryIdException_WhenIdIsEmpty()
    {
        var command = new UpdateCategoryCommand
        {
            Id = Guid.Empty,
            Name = "Novo nome"
        };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await Assert.ThrowsAsync<InvalidCategoryIdException>(act);
    }

    [Fact]
    public async Task Handle_ShouldThrowCategoryNotFoundException_WhenCategoryDoesNotExist()
    {
        var categoryId = Guid.NewGuid();
        var command = new UpdateCategoryCommand
        {
            Id = categoryId,
            Name = "Novo nome"
        };

        _repositoryMock
            .Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await Assert.ThrowsAsync<CategoryNotFoundException>(act);
    }

    [Fact]
    public async Task Handle_ShouldUpdateCategory_AndReturnId()
    {
        var category = new Category("Hardware");
        var command = new UpdateCategoryCommand
        {
            Id = category.Id,
            Name = "Monitores"
        };

        _repositoryMock
            .Setup(x => x.GetByIdAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _repositoryMock
            .Setup(x => x.UpdateAsync(category, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.Equal(category.Id, result);
        Assert.Equal("Monitores", category.Name);
    }
}

public class DeleteCategoryHandlerTests
{
    private readonly Mock<ICategoryRepository> _repositoryMock;
    private readonly DeleteCategoryHandler _handler;

    public DeleteCategoryHandlerTests()
    {
        _repositoryMock = new Mock<ICategoryRepository>();
        _handler = new DeleteCategoryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidCategoryIdException_WhenIdIsEmpty()
    {
        var command = new DeleteCategoryCommand(Guid.Empty);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await Assert.ThrowsAsync<InvalidCategoryIdException>(act);
    }

    [Fact]
    public async Task Handle_ShouldThrowCategoryNotFoundException_WhenCategoryDoesNotExist()
    {
        var categoryId = Guid.NewGuid();
        var command = new DeleteCategoryCommand(categoryId);

        _repositoryMock
            .Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await Assert.ThrowsAsync<CategoryNotFoundException>(act);
    }

    [Fact]
    public async Task Handle_ShouldDeleteCategory_AndReturnId()
    {
        var category = new Category("Hardware");
        var command = new DeleteCategoryCommand(category.Id);

        _repositoryMock
            .Setup(x => x.GetByIdAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _repositoryMock
            .Setup(x => x.DeleteAsync(category, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.Equal(category.Id, result);

        _repositoryMock.Verify(
            x => x.DeleteAsync(category, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
