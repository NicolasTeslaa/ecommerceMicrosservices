using NotaFiscal.Domain.Entities;

namespace NotaFiscal.Application.Interfaces;

public interface IInvoiceEventPublisher
{
    Task PublishIssuedAsync(Invoice invoice, CancellationToken cancellationToken = default);
}
