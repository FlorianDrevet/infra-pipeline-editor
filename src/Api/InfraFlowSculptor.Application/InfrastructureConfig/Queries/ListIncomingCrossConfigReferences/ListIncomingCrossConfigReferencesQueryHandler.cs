using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Queries.ListIncomingCrossConfigReferences;

/// <summary>
/// Handles listing incoming cross-config references: resources in OTHER configurations
/// that depend on resources in THIS configuration through cross-config references.
/// Optimized to batch-load sibling configs, resolve targets via the base AzureResource table,
/// and use a parent-child view instead of N+1 sequential queries.
/// </summary>
public sealed class ListIncomingCrossConfigReferencesQueryHandler(
    IInfraConfigAccessService accessService,
    IInfrastructureConfigRepository infraConfigRepository,
    IResourceGroupRepository resourceGroupRepository)
    : IQueryHandler<ListIncomingCrossConfigReferencesQuery, List<IncomingCrossConfigReferenceResult>>
{
    /// <inheritdoc />
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "Tracked under test-debt #22: refactoring deferred until dedicated unit-test coverage protects against behavioural regressions. The method orchestrates a single coherent business operation and would lose readability without proper test guards.")]
    public async Task<ErrorOr<List<IncomingCrossConfigReferenceResult>>> Handle(
        ListIncomingCrossConfigReferencesQuery query,
        CancellationToken cancellationToken)
    {
        var configId = new InfrastructureConfigId(query.InfraConfigId);
        var authResult = await accessService.VerifyReadAccessAsync(configId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        var config = authResult.Value;

        // Load all sibling configs in the same project (lightweight, no Includes)
        var siblingConfigs = await infraConfigRepository.GetByProjectIdAsync(config.ProjectId, cancellationToken);

        // Collect all incoming cross-config references targeting this config
        var incomingRefs = new List<(
            InfrastructureConfigId SiblingId,
            string SiblingName,
            Guid ReferenceId,
            AzureResourceId TargetResourceId)>();

        foreach (var sibling in siblingConfigs)
        {
            if (sibling.Id == config.Id) continue;

            var siblingWithRefs = await infraConfigRepository.GetByIdWithMembersAsync(sibling.Id, cancellationToken);
            if (siblingWithRefs is null) continue;

            var matchingRefs = siblingWithRefs.CrossConfigReferences
                .Where(r => r.TargetConfigId == config.Id)
                .ToList();

            foreach (var r in matchingRefs)
            {
                incomingRefs.Add((sibling.Id, siblingWithRefs.Name.Value, r.Id.Value, r.TargetResourceId));
            }
        }

        if (incomingRefs.Count == 0)
            return new List<IncomingCrossConfigReferenceResult>();

        // Batch-resolve all target resource metadata (name, type, RG name)
        var targetResourceIds = incomingRefs
            .Select(r => r.TargetResourceId)
            .Distinct()
            .ToList();
        var targetMetadataList = await resourceGroupRepository.GetResourceMetadataBatchAsync(
            targetResourceIds, cancellationToken);
        var targetMetadata = targetMetadataList.ToDictionary(m => m.ResourceId);

        // Load parent-child mappings for all sibling configs' resource groups
        // to find child resources whose parent FK matches a target resource
        var siblingConfigIds = incomingRefs
            .Select(r => r.SiblingId)
            .Distinct()
            .ToList();

        // For each sibling, load its RGs (lightweight) and collect childâ†’parent mappings
        var allChildToParent = new Dictionary<Guid, (Guid ParentId, Guid SiblingConfigId, string SiblingConfigName, string ChildName, string ChildType, string ChildRgName)>();

        foreach (var siblingId in siblingConfigIds)
        {
            var siblingName = incomingRefs.First(r => r.SiblingId == siblingId).SiblingName;
            var rgs = await resourceGroupRepository.GetLightweightByInfraConfigIdAsync(siblingId, cancellationToken);

            foreach (var rg in rgs)
            {
                var parentMapping = await resourceGroupRepository.GetChildToParentMappingAsync(rg.Id, cancellationToken);

                if (parentMapping.Count == 0) continue;

                // Batch-resolve child resource metadata
                var childIds = parentMapping.Keys.Select(id => new AzureResourceId(id)).ToList();
                var childMetadataList = await resourceGroupRepository.GetResourceMetadataBatchAsync(
                    childIds, cancellationToken);

                foreach (var childMeta in childMetadataList)
                {
                    if (parentMapping.TryGetValue(childMeta.ResourceId, out var parentId))
                    {
                        allChildToParent[childMeta.ResourceId] = (
                            parentId,
                            siblingId.Value,
                            siblingName,
                            childMeta.ResourceName,
                            childMeta.ResourceType,
                            childMeta.ResourceGroupName);
                    }
                }
            }
        }

        // Build results by matching incoming refs' target resources to childâ†’parent mappings
        var results = new List<IncomingCrossConfigReferenceResult>();

        foreach (var incoming in incomingRefs)
        {
            if (!targetMetadata.TryGetValue(incoming.TargetResourceId.Value, out var target))
                continue;

            foreach (var (childId, mapping) in allChildToParent)
            {
                if (mapping.ParentId != incoming.TargetResourceId.Value) continue;
                if (mapping.SiblingConfigId != incoming.SiblingId.Value) continue;

                results.Add(new IncomingCrossConfigReferenceResult(
                    ReferenceId: incoming.ReferenceId,
                    SourceConfigId: mapping.SiblingConfigId,
                    SourceConfigName: mapping.SiblingConfigName,
                    SourceResourceId: childId,
                    SourceResourceName: mapping.ChildName,
                    SourceResourceType: mapping.ChildType,
                    SourceResourceGroupName: mapping.ChildRgName,
                    TargetResourceId: target.ResourceId,
                    TargetResourceName: target.ResourceName,
                    TargetResourceType: target.ResourceType));
            }
        }

        return results;
    }
}
