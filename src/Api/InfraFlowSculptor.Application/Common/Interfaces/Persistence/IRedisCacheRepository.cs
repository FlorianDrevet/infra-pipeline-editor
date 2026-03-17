using InfraFlowSculptor.Domain.RedisCacheAggregate;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using Shared.Application.Interfaces;

namespace InfraFlowSculptor.Application.Common.Interfaces.Persistence;

public interface IRedisCacheRepository : IRepository<RedisCache>
{
    Task<List<RedisCache>> GetByResourceGroupIdAsync(ResourceGroupId resourceGroupId, CancellationToken cancellationToken = default);
}
