using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Application.RedisCaches.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.RedisCaches.Queries;

public record GetRedisCacheQuery(
    AzureResourceId Id
) : IQuery<RedisCacheResult>;
