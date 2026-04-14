using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Application.RedisCaches.Common;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.RedisCaches.Queries;

public record ListRedisCachesQuery(
    ResourceGroupId ResourceGroupId
) : IQuery<List<RedisCacheResult>>;
