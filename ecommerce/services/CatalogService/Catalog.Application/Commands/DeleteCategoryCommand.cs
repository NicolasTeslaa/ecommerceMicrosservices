using MediatR;

namespace Catalog.Application.Commands;

public class DeleteCategoryCommand : IRequest<Guid>
{
    public Guid Id { get; set; }

    public DeleteCategoryCommand(Guid id)
    {
        Id = id;
    }
}
