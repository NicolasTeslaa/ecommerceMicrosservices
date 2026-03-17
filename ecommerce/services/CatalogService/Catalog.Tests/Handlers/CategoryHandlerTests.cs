using Catalog.Application.Commands;
using Catalog.Application.Handlers;
using Catalog.Application.Interfaces;
using Catalog.Application.Queries;
using Catalog.Application.ReadModels;
using Catalog.Domain.Entities;
using Catalog.Domain.Exceptions;
using ECommerce.Shared.Contracts;
using Moq;

namespace Catalog.Tests.Handlers;

public class CreateCategoryHandlerTests
{
    private readonly Mock<ICategoryWriteRepository> _repositoryMock;
    private readonly Mock<ICategoryReadModelProjector> _projectorMock;
    private readonly CreateCategoryHandler _handler;

    public CreateCategoryHandlerTests()
    {
        _repositoryMock = new Mock<ICategoryWriteRepository>();
        _projectorMock = new Mock<ICategoryReadModelProjector>();
        _handler = new CreateCategoryHandler(_repositoryMock.Object, _projectorMock.Object);
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

        _projectorMock
            .Setup(x => x.UpsertAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, result);
        Assert.NotNull(capturedCategory);
        Assert.Equal("Hardware", capturedCategory!.Name);
        Assert.Equal(capturedCategory.Id, result);

        _projectorMock.Verify(
            x => x.UpsertAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}

public class GetCategoryByIdHandlerTests
{
    private readonly Mock<ICategoryReadRepository> _repositoryMock;
    private readonly GetCategoryByIdHandler _handler;

    public GetCategoryByIdHandlerTests()
    {
        _repositoryMock = new Mock<ICategoryReadRepository>();
        _handler = new GetCategoryByIdHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidCategoryIdException_WhenIdIsEmpty()
    {
        var query = new GetCategoryByIdQuery(Guid.Empty);

        var act = () => _handler.Handle(query, CancellationToken.None);

        await Assert.ThrowsAsync<InvalidCategoryIdException>(act);

        _repositoryMock.Verify(
            x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowCategoryNotFoundException_WhenCategoryDoesNotExist()
    {
        var categoryId = Guid.NewGuid();
        var query = new GetCategoryByIdQuery(categoryId);

        _repositoryMock
            .Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CategoryReadModel?)null);

        var act = () => _handler.Handle(query, CancellationToken.None);

        await Assert.ThrowsAsync<CategoryNotFoundException>(act);
    }

    [Fact]
    public async Task Handle_ShouldReturnMappedCategory_WhenCategoryExists()
    {
        var category = new CategoryReadModel
        {
            Id = Guid.NewGuid(),
            Name = "Hardware"
        };
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
    private readonly Mock<ICategoryReadRepository> _repositoryMock;
    private readonly GetAllCategoriesHandler _handler;

    public GetAllCategoriesHandlerTests()
    {
        _repositoryMock = new Mock<ICategoryReadRepository>();
        _handler = new GetAllCategoriesHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnMappedCategories()
    {
        var query = new GetAllCategoriesQuery
        {
            PageNumber = 1,
            PageSize = 10
        };

        var categories = new List<CategoryReadModel>
        {
            new() { Id = Guid.NewGuid(), Name = "Hardware" },
            new() { Id = Guid.NewGuid(), Name = "Monitores" }
        };

        _repositoryMock
            .Setup(x => x.GetAllAsync(
                It.Is<PaginationRequest>(pagination => pagination.PageNumber == query.PageNumber && pagination.PageSize == query.PageSize),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(PagedResult<CategoryReadModel>.Create(categories, query.PageNumber, query.PageSize, categories.Count));

        var result = await _handler.Handle(query, CancellationToken.None);
        var list = result.Items.ToList();

        Assert.Equal(2, list.Count);
        Assert.Equal(categories[0].Id, list[0].Id);
        Assert.Equal(categories[0].Name, list[0].Name);
        Assert.Equal(categories[1].Id, list[1].Id);
        Assert.Equal(categories[1].Name, list[1].Name);
        Assert.Equal(categories.Count, result.Pagination.TotalItems);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenRepositoryReturnsEmpty()
    {
        var query = new GetAllCategoriesQuery
        {
            PageNumber = 2,
            PageSize = 5
        };

        _repositoryMock
            .Setup(x => x.GetAllAsync(
                It.Is<PaginationRequest>(pagination => pagination.PageNumber == query.PageNumber && pagination.PageSize == query.PageSize),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(PagedResult<CategoryReadModel>.Create(Enumerable.Empty<CategoryReadModel>(), query.PageNumber, query.PageSize, 0));

        var result = await _handler.Handle(query, CancellationToken.None);

        Assert.Empty(result.Items);
        Assert.Equal(0, result.Pagination.TotalItems);

        _repositoryMock.Verify(
            x => x.GetAllAsync(
                It.Is<PaginationRequest>(pagination => pagination.PageNumber == query.PageNumber && pagination.PageSize == query.PageSize),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}

public class UpdateCategoryHandlerTests
{
    private readonly Mock<ICategoryWriteRepository> _repositoryMock;
    private readonly Mock<ICategoryReadModelProjector> _projectorMock;
    private readonly UpdateCategoryHandler _handler;

    public UpdateCategoryHandlerTests()
    {
        _repositoryMock = new Mock<ICategoryWriteRepository>();
        _projectorMock = new Mock<ICategoryReadModelProjector>();
        _handler = new UpdateCategoryHandler(_repositoryMock.Object, _projectorMock.Object);
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

        _projectorMock.Verify(
            x => x.UpsertAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _repositoryMock.Verify(
            x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
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

        _projectorMock.Verify(
            x => x.UpsertAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()),
            Times.Never);
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

        _projectorMock
            .Setup(x => x.UpsertAsync(category, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.Equal(category.Id, result);
        Assert.Equal("Monitores", category.Name);

        _projectorMock.Verify(
            x => x.UpsertAsync(category, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}

public class DeleteCategoryHandlerTests
{
    private readonly Mock<ICategoryWriteRepository> _repositoryMock;
    private readonly Mock<ICategoryReadModelProjector> _projectorMock;
    private readonly DeleteCategoryHandler _handler;

    public DeleteCategoryHandlerTests()
    {
        _repositoryMock = new Mock<ICategoryWriteRepository>();
        _projectorMock = new Mock<ICategoryReadModelProjector>();
        _handler = new DeleteCategoryHandler(_repositoryMock.Object, _projectorMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidCategoryIdException_WhenIdIsEmpty()
    {
        var command = new DeleteCategoryCommand(Guid.Empty);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await Assert.ThrowsAsync<InvalidCategoryIdException>(act);

        _projectorMock.Verify(
            x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _repositoryMock.Verify(
            x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
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

        _projectorMock.Verify(
            x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
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

        _projectorMock
            .Setup(x => x.DeleteAsync(category.Id, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.Equal(category.Id, result);

        _repositoryMock.Verify(
            x => x.DeleteAsync(category, It.IsAny<CancellationToken>()),
            Times.Once);

        _projectorMock.Verify(
            x => x.DeleteAsync(category.Id, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
