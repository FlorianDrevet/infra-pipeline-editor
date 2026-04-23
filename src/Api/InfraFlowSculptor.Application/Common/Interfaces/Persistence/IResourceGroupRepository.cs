using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.ResourceGroups.Common;

namespace InfraFlowSculptor.Application.Common.Interfaces.Persistence;

public interface IResourceGroupRepository: IRepository<Domain.ResourceGroupAggregate.ResourceGroup>
{
    Task<Domain.ResourceGroupAggregate.ResourceGroup?> GetByIdWithResourcesAsync(ResourceGroupId id, CancellationToken ct = default);
    Task<List<Domain.ResourceGroupAggregate.ResourceGroup>> GetByInfraConfigIdAsync(
        InfrastructureConfigId infraConfigId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns resource groups for the given infrastructure config without loading resources.
    /// Use this lightweight projection when only resource-group-level fields are needed.
    /// </summary>
    Task<List<Domain.ResourceGroupAggregate.ResourceGroup>> GetLightweightByInfraConfigIdAsync(
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
    Task<Domain.ResourceGroupAggregate.ResourceGroup?> GetByContainedResourceIdAsync(
        AzureResourceId resourceId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a mapping of resource IDs to their configured environment names,
    /// by querying the <c>vw_ResourceEnvironmentEntries</c> view for resources in the given resource group.
    /// </summary>
    Task<Dictionary<Guid, List<string>>> GetConfiguredEnvironmentsByResourceGroupAsync(
        ResourceGroupId resourceGroupId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns lightweight resource summaries (no TPT resolution) for all resources in the given resource group.
    /// </summary>
    Task<List<ResourceSummary>> GetResourceSummariesByGroupIdAsync(
        ResourceGroupId resourceGroupId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns lightweight Storage Account child resources for the provided Storage Account identifiers.
    /// Uses narrow child-table projections to avoid loading full Storage Account aggregates.
    /// </summary>
    /// <param name="storageAccountIds">Identifiers of the Storage Accounts to enrich.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A mapping of Storage Account identifier to its lightweight child resources.</returns>
    Task<Dictionary<Guid, StorageAccountSubResourcesResult>> GetStorageSubResourcesByStorageAccountIdsAsync(
        IReadOnlyList<AzureResourceId> storageAccountIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the distinct Azure resource type names used across all configurations of a project.
    /// </summary>
    Task<List<string>> GetDistinctResourceTypesByProjectIdAsync(
        Domain.ProjectAggregate.ValueObjects.ProjectId projectId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns lightweight resource metadata (name, type, resource group name) for multiple resource IDs in a single query.
    /// Used to batch-resolve cross-config reference targets without N+1 queries.
    /// </summary>
    Task<List<ResourceMetadata>> GetResourceMetadataBatchAsync(
        IReadOnlyList<AzureResourceId> resourceIds,
        CancellationToken cancellationToken = default);
}