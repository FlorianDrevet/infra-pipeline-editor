using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.KeyVaultAggregate;
using InfraFlowSculptor.Domain.ResourceGroupAggregate;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Domain.Models;
using Shared.Infrastructure.Persistence.Repositories;

namespace InfraFlowSculptor.Infrastructure.Persistence.Repositories;

public class ResourceGroupRepository: BaseRepository<ResourceGroup, ProjectDbContext>, IResourceGroupRepository
{
    public ResourceGroupRepository(ProjectDbContext context) : base(context)
    {
    }

    public override async Task<ResourceGroup?> GetByIdAsync(ValueObject id, CancellationToken cancellationToken)
    {
        return await Context.Set<ResourceGroup>()
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }
}