using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.KeyVaultAggregate;
using InfraFlowSculptor.Domain.ResourceGroupAggregate;

namespace InfraFlowSculptor.Infrastructure.Persistence.Repositories;

public class ResourceGroupRepository: BaseRepository<ResourceGroup, ProjectDbContext>, IResourceGroupRepository
{
    public ResourceGroupRepository(ProjectDbContext context) : base(context)
    {
    }
}