using Expedition.Application.DTOs;
using MediatR;

namespace Expedition.Application.Commands;

public record MarkExpeditionPickedUpCommand(Guid OrderId) : IRequest<ExpeditionDto>;
