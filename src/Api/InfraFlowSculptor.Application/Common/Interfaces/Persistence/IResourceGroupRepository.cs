using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
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

    /// <summary>
    /// Finds the resource group that contains the specified Azure resource.
    /// </summary>
    /// <param name="resourceId">The Azure resource identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The resource group, or null if the resource was not found.</returns>
    Task<Domain.ResourceGroupAggregate.ResourceGroup?> GetByResourceIdAsync(
        AzureResourceId resourceId,
        CancellationToken cancellationToken = default);
}