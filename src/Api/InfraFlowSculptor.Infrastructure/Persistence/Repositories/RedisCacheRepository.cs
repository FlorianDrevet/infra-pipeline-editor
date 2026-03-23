using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.RedisCacheAggregate;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Domain.Models;
using Shared.Infrastructure.Persistence.Repositories;

namespace InfraFlowSculptor.Infrastructure.Persistence.Repositories;

public class RedisCacheRepository : AzureResourceRepository<RedisCache>, IRedisCacheRepository
{
    public RedisCacheRepository(ProjectDbContext context) : base(context)
    {
    }

    public override async Task<RedisCache?> GetByIdAsync(ValueObject id, CancellationToken cancellationToken)
    {
        return await Context.Set<RedisCache>()
            .Include(rc => rc.DependsOn)
            .Include(rc => rc.EnvironmentSettings)
            .FirstOrDefaultAsync(rc => rc.Id == id, cancellationToken);
    }

    public async Task<List<RedisCache>> GetByResourceGroupIdAsync(ResourceGroupId resourceGroupId, CancellationToken cancellationToken = default)
    {
        return await Context.Set<RedisCache>()
            .Include(rc => rc.DependsOn)
            .Include(rc => rc.EnvironmentSettings)
            .Where(rc => rc.ResourceGroupId == resourceGroupId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
