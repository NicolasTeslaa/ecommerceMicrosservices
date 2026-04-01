using Expedition.Application.DTOs;
using MediatR;

namespace Expedition.Application.Queries;

public record GetExpeditionByOrderIdQuery(Guid OrderId) : IRequest<ExpeditionDto?>;
