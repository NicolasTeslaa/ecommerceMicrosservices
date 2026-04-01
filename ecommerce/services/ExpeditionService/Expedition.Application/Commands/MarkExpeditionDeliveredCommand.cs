using Expedition.Application.DTOs;
using MediatR;

namespace Expedition.Application.Commands;

public record MarkExpeditionDeliveredCommand(Guid OrderId) : IRequest<ExpeditionDto>;
