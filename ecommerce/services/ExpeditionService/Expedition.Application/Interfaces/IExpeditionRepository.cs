using Expedition.Application.DTOs;
using Expedition.Domain.Entities;

namespace Expedition.Application.Interfaces;

public interface IExpeditionRepository
{
    Task<ExpeditionDto?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<ExpeditionOrder?> GetEntityByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task AddAsync(ExpeditionOrder expeditionOrder, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
