using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using Shared.Application.Interfaces;

namespace InfraFlowSculptor.Application.Common.Interfaces.Persistence;

public interface IResourceGroupRepository: IRepository<Domain.ResourceGroupAggregate.ResourceGroup>
{
    Task<Domain.ResourceGroupAggregate.ResourceGroup?> GetByIdWithResourcesAsync(ResourceGroupId id, CancellationToken ct = default);
}