namespace Catalog.Application.Commands;

public class DeactivateProductCommand : MediatR.IRequest<Guid>
{
    public Guid Id { get; set; }

    public DeactivateProductCommand(Guid id)
    {
        Id = id;
    }
}
