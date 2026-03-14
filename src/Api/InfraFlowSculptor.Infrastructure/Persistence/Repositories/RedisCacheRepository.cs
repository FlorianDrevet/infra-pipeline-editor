using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.RedisCacheAggregate;

namespace InfraFlowSculptor.Infrastructure.Persistence.Repositories;

public class RedisCacheRepository : AzureResourceRepository<RedisCache>, IRedisCacheRepository
{
    public RedisCacheRepository(ProjectDbContext context) : base(context)
    {
    }
}
