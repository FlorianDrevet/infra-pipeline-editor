using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.RedisCaches.Commands.DeleteRedisCache;

public record DeleteRedisCacheCommand(
    AzureResourceId Id
) : ICommand<Deleted>;
