using MediatR;
using NotaFiscal.Application.DTOs;
using NotaFiscal.Application.Interfaces;
using NotaFiscal.Application.Queries;

namespace NotaFiscal.Application.Handlers;

public class GetInvoiceByOrderIdHandler : IRequestHandler<GetInvoiceByOrderIdQuery, InvoiceDto?>
{
    private readonly IInvoiceRepository _repository;

    public GetInvoiceByOrderIdHandler(IInvoiceRepository repository)
    {
        _repository = repository;
    }

    public Task<InvoiceDto?> Handle(GetInvoiceByOrderIdQuery request, CancellationToken cancellationToken)
    {
        return _repository.GetByOrderIdAsync(request.OrderId, cancellationToken);
    }
}
