using Expedition.Application.DTOs;
using MediatR;

namespace Expedition.Application.Commands;

public record MarkExpeditionInTransitCommand(Guid OrderId) : IRequest<ExpeditionDto>;
