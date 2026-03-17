using Catalog.Application.Commands;
using Catalog.Application.Interfaces;
using Catalog.Domain.Exceptions;
using MediatR;

namespace Catalog.Application.Handlers;

public class UpdateProductHandler : IRequestHandler<UpdateProductCommand, Guid>
{
    private readonly IProductWriteRepository _repository;
    private readonly IProductReadModelProjector _projector;
    private readonly ICategoryWriteRepository _categoryRepository;

    public UpdateProductHandler(
        IProductWriteRepository repository,
        IProductReadModelProjector projector,
        ICategoryWriteRepository categoryRepository)
    {
        _repository = repository;
        _projector = projector;
        _categoryRepository = categoryRepository;
    }

    public async Task<Guid> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        if (request.Id == Guid.Empty)
            throw new InvalidProductIdException();

        if (request.CategoryId == Guid.Empty)
            throw new InvalidCategoryIdException();

        var product = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (product is null)
            throw new ProductNotFoundException(request.Id);

        var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);

        if (category is null)
            throw new CategoryNotFoundException(request.CategoryId);

        product.Update(
            request.Name,
            request.Description,
            request.Price,
            request.StockQuantity,
            request.CategoryId);

        await _repository.UpdateAsync(product, cancellationToken);
        await _projector.UpsertAsync(product, cancellationToken);

        return product.Id;
    }
}
