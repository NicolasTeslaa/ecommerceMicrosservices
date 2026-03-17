using Catalog.Application.Commands;
using Catalog.Application.Interfaces;
using Catalog.Domain.Entities;
using Catalog.Domain.Exceptions;
using MediatR;

namespace Catalog.Application.Handlers;

public class CreateProductHandler : IRequestHandler<CreateProductCommand, Guid>
{
    private readonly IProductWriteRepository _repository;
    private readonly IProductReadModelProjector _projector;
    private readonly ICategoryWriteRepository _categoryRepository;

    public CreateProductHandler(
        IProductWriteRepository repository,
        IProductReadModelProjector projector,
        ICategoryWriteRepository categoryRepository)
    {
        _repository = repository;
        _projector = projector;
        _categoryRepository = categoryRepository;
    }

    public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        if (request.CategoryId == Guid.Empty)
            throw new InvalidCategoryIdException();

        var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);

        if (category is null)
            throw new CategoryNotFoundException(request.CategoryId);

        var product = new Product(
            request.Name,
            request.Description,
            request.Price,
            request.StockQuantity,
            request.CategoryId);

        await _repository.AddAsync(product, cancellationToken);
        await _projector.UpsertAsync(product, cancellationToken);

        return product.Id;
    }
}
