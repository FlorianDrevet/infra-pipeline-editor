using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.KeyVaultAggregate;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using InfraFlowSculptor.Domain.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace InfraFlowSculptor.Infrastructure.Persistence.Repositories;

public class KeyVaultRepository: AzureResourceRepository<KeyVault>, IKeyVaultRepository
{
    public KeyVaultRepository(ProjectDbContext context) : base(context)
    {
    }

    public override async Task<KeyVault?> GetByIdAsync(ValueObject id, CancellationToken cancellationToken = default)
    {
        return await WithSubResources(Context.Set<KeyVault>())
            .FirstOrDefaultAsync(kv => kv.Id == id, cancellationToken);
    }

    public override async Task<KeyVault?> GetByIdReadOnlyAsync(ValueObject id, CancellationToken cancellationToken = default)
    {
        return await WithSubResources(Context.Set<KeyVault>().AsNoTracking())
            .FirstOrDefaultAsync(kv => kv.Id == id, cancellationToken);
    }

    public async Task<List<KeyVault>> GetByResourceGroupIdAsync(ResourceGroupId resourceGroupId, CancellationToken cancellationToken = default)
    {
        return await WithSubResources(Context.Set<KeyVault>())
            .Where(kv => kv.ResourceGroupId == resourceGroupId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    private static IQueryable<KeyVault> WithSubResources(IQueryable<KeyVault> query)
    {
        return query
            .Include(kv => kv.DependsOn)
            .Include(kv => kv.EnvironmentSettings);
    }
}