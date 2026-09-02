using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.RedisCacheAggregate;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using InfraFlowSculptor.Domain.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace InfraFlowSculptor.Infrastructure.Persistence.Repositories;

public class RedisCacheRepository : AzureResourceRepository<RedisCache>, IRedisCacheRepository
{
    public RedisCacheRepository(ProjectDbContext context) : base(context)
    {
    }

    public override async Task<RedisCache?> GetByIdAsync(ValueObject id, CancellationToken cancellationToken = default)
    {
        return await WithSubResources(Context.Set<RedisCache>())
            .FirstOrDefaultAsync(rc => rc.Id == id, cancellationToken);
    }

    public override async Task<RedisCache?> GetByIdReadOnlyAsync(ValueObject id, CancellationToken cancellationToken = default)
    {
        return await WithSubResources(Context.Set<RedisCache>().AsNoTracking())
            .Include(rc => rc.ResourceGroup)
            .FirstOrDefaultAsync(rc => rc.Id == id, cancellationToken);
    }

    public async Task<List<RedisCache>> GetByResourceGroupIdAsync(ResourceGroupId resourceGroupId, CancellationToken cancellationToken = default)
    {
        return await WithSubResources(Context.Set<RedisCache>())
            .Where(rc => rc.ResourceGroupId == resourceGroupId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    private static IQueryable<RedisCache> WithSubResources(IQueryable<RedisCache> query)
    {
        return query
            .Include(rc => rc.DependsOn)
            .Include(rc => rc.EnvironmentSettings);
    }
}
