using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.KeyVaultAggregate;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using Microsoft.EntityFrameworkCore;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Infrastructure.Persistence.Repositories;

namespace InfraFlowSculptor.Infrastructure.Persistence.Repositories;

public class KeyVaultRepository: AzureResourceRepository<KeyVault>, IKeyVaultRepository
{
    public KeyVaultRepository(ProjectDbContext context) : base(context)
    {
    }

    public override async Task<KeyVault?> GetByIdAsync(ValueObject id, CancellationToken cancellationToken)
    {
        return await Context.Set<KeyVault>()
            .Include(kv => kv.DependsOn)
            .Include(kv => kv.EnvironmentSettings)
            .FirstOrDefaultAsync(kv => kv.Id == id, cancellationToken);
    }

    public async Task<List<KeyVault>> GetByResourceGroupIdAsync(ResourceGroupId resourceGroupId, CancellationToken cancellationToken = default)
    {
        return await Context.Set<KeyVault>()
            .Include(kv => kv.DependsOn)
            .Include(kv => kv.EnvironmentSettings)
            .Where(kv => kv.ResourceGroupId == resourceGroupId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}