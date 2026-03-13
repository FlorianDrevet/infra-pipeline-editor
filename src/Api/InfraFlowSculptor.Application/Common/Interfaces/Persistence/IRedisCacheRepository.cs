using InfraFlowSculptor.Domain.RedisCacheAggregate;
using Shared.Application.Interfaces;

namespace InfraFlowSculptor.Application.Common.Interfaces.Persistence;

public interface IRedisCacheRepository : IRepository<RedisCache>
{
}
