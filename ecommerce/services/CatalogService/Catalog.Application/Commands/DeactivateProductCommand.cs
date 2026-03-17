using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catalog.Application.Commands
{
    public class DeactivateProductCommand : MediatR.IRequest<Guid>
{
    public Guid Id { get; set; }

    public DeactivateProductCommand(Guid id)
    {
        Id = id;
    }
}
}
