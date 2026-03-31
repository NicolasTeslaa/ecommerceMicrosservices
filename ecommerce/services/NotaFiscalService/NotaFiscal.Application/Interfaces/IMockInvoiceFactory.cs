using ECommerce.Shared.Messaging;
using NotaFiscal.Domain.Entities;

namespace NotaFiscal.Application.Interfaces;

public interface IMockInvoiceFactory
{
    Invoice Create(OrderConfirmedIntegrationEvent integrationEvent);
}
