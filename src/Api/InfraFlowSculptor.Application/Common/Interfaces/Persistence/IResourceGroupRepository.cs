using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using InfraFlowSculptor.Application.Common.Interfaces;

namespace InfraFlowSculptor.Application.Common.Interfaces.Persistence;

public interface IResourceGroupRepository: IRepository<Domain.ResourceGroupAggregate.ResourceGroup>
{
    Task<Domain.ResourceGroupAggregate.ResourceGroup?> GetByIdWithResourcesAsync(ResourceGroupId id, CancellationToken ct = default);
    Task<List<Domain.ResourceGroupAggregate.ResourceGroup>> GetByInfraConfigIdAsync(
        InfrastructureConfigId infraConfigId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns resource group count and total resource count per infrastructure config ID.
    /// </summary>
    Task<Dictionary<Guid, (int ResourceGroupCount, int ResourceCount)>> GetResourceCountsByInfraConfigIdsAsync(
        IReadOnlyList<InfrastructureConfigId> infraConfigIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a mapping of child resource IDs to their parent resource IDs
    /// by querying child-type TPT tables directly (WebApp, FunctionApp, ContainerApp, SqlDatabase, ApplicationInsights).
    /// </summary>
    Task<Dictionary<Guid, Guid>> GetChildToParentMappingAsync(
        ResourceGroupId resourceGroupId,
        CancellationToken cancellationToken = default);
}