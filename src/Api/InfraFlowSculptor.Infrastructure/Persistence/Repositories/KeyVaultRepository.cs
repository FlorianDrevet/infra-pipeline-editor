using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.KeyVaultAggregate;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Persistence.Repositories;

namespace InfraFlowSculptor.Infrastructure.Persistence.Repositories;

public class KeyVaultRepository: AzureResourceRepository<KeyVault>, IKeyVaultRepository
{
    public KeyVaultRepository(ProjectDbContext context) : base(context)
    {
    }

    public async Task<List<KeyVault>> GetByResourceGroupIdAsync(ResourceGroupId resourceGroupId, CancellationToken cancellationToken = default)
    {
        return await Context.Set<KeyVault>()
            .Include(kv => kv.DependsOn)
            .Where(kv => kv.ResourceGroupId == resourceGroupId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}