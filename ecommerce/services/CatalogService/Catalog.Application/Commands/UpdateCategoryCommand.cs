using MediatR;

namespace Catalog.Application.Commands;

public class UpdateCategoryCommand : IRequest<Guid>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
