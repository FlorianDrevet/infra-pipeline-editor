using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.RedisCacheAggregate;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace InfraFlowSculptor.Infrastructure.Persistence.Repositories;

public class RedisCacheRepository : AzureResourceRepository<RedisCache>, IRedisCacheRepository
{
    public RedisCacheRepository(ProjectDbContext context) : base(context)
    {
    }

    public async Task<List<RedisCache>> GetByResourceGroupIdAsync(ResourceGroupId resourceGroupId, CancellationToken cancellationToken = default)
    {
        return await Context.Set<RedisCache>()
            .Include(rc => rc.DependsOn)
            .Where(rc => rc.ResourceGroupId == resourceGroupId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
