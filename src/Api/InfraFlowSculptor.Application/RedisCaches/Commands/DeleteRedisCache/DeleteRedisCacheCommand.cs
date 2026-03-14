using ErrorOr;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.RedisCaches.Commands.DeleteRedisCache;

public record DeleteRedisCacheCommand(
    AzureResourceId Id
) : IRequest<ErrorOr<Deleted>>;
