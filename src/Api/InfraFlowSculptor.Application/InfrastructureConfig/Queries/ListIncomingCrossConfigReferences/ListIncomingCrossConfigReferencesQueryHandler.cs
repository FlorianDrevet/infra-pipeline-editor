using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.ResourceGroups.Common;
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
    public async Task<ErrorOr<List<IncomingCrossConfigReferenceResult>>> Handle(
        ListIncomingCrossConfigReferencesQuery query,
        CancellationToken cancellationToken)
    {
        var configId = new InfrastructureConfigId(query.InfraConfigId);
        var authResult = await accessService.VerifyReadAccessAsync(configId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        var config = authResult.Value;

        var incomingRefs = await CollectIncomingReferencesAsync(config, cancellationToken);
        if (incomingRefs.Count == 0)
            return new List<IncomingCrossConfigReferenceResult>();

        var targetMetadata = await LoadTargetMetadataAsync(incomingRefs, cancellationToken);
        var childToParent = await BuildChildToParentMappingAsync(incomingRefs, cancellationToken);

        return BuildResults(incomingRefs, targetMetadata, childToParent);
    }

    private async Task<List<IncomingReferenceRecord>> CollectIncomingReferencesAsync(
        Domain.InfrastructureConfigAggregate.InfrastructureConfig config,
        CancellationToken cancellationToken)
    {
        var siblingConfigs = await infraConfigRepository.GetByProjectIdAsync(config.ProjectId, cancellationToken);
        var incomingRefs = new List<IncomingReferenceRecord>();

        foreach (var siblingId in siblingConfigs.Select(sibling => sibling.Id))
        {
            if (siblingId == config.Id) continue;

            var siblingWithRefs = await infraConfigRepository.GetByIdWithMembersAsync(siblingId, cancellationToken);
            if (siblingWithRefs is null) continue;

            foreach (var r in siblingWithRefs.CrossConfigReferences.Where(r => r.TargetConfigId == config.Id))
            {
                incomingRefs.Add(new IncomingReferenceRecord(
                    siblingId,
                    siblingWithRefs.Name.Value,
                    r.Id.Value,
                    r.TargetResourceId));
            }
        }

        return incomingRefs;
    }

    private async Task<Dictionary<Guid, ResourceMetadata>> LoadTargetMetadataAsync(
        List<IncomingReferenceRecord> incomingRefs,
        CancellationToken cancellationToken)
    {
        var targetResourceIds = incomingRefs
            .Select(r => r.TargetResourceId)
            .Distinct()
            .ToList();
        var targetMetadataList = await resourceGroupRepository.GetResourceMetadataBatchAsync(
            targetResourceIds, cancellationToken);
        return targetMetadataList.ToDictionary(m => m.ResourceId);
    }

    private async Task<Dictionary<Guid, ChildToParentRecord>> BuildChildToParentMappingAsync(
        List<IncomingReferenceRecord> incomingRefs,
        CancellationToken cancellationToken)
    {
        var siblingConfigIds = incomingRefs
            .Select(r => r.SiblingId)
            .Distinct()
            .ToList();

        var allChildToParent = new Dictionary<Guid, ChildToParentRecord>();

        foreach (var siblingId in siblingConfigIds)
        {
            var siblingName = incomingRefs
                .Where(r => r.SiblingId == siblingId)
                .Select(r => r.SiblingName)
                .FirstOrDefault();

            if (string.IsNullOrWhiteSpace(siblingName))
                continue;

            await AppendSiblingMappingAsync(siblingId, siblingName, allChildToParent, cancellationToken);
        }

        return allChildToParent;
    }

    private async Task AppendSiblingMappingAsync(
        InfrastructureConfigId siblingId,
        string siblingName,
        Dictionary<Guid, ChildToParentRecord> allChildToParent,
        CancellationToken cancellationToken)
    {
        var rgs = await resourceGroupRepository.GetLightweightByInfraConfigIdAsync(siblingId, cancellationToken);

        foreach (var rg in rgs)
        {
            var parentMapping = await resourceGroupRepository.GetChildToParentMappingAsync(rg.Id, cancellationToken);
            if (parentMapping.Count == 0) continue;

            var childIds = parentMapping.Keys.Select(id => new AzureResourceId(id)).ToList();
            var childMetadataList = await resourceGroupRepository.GetResourceMetadataBatchAsync(
                childIds, cancellationToken);

            foreach (var childMeta in childMetadataList)
            {
                if (parentMapping.TryGetValue(childMeta.ResourceId, out var parentId))
                {
                    allChildToParent[childMeta.ResourceId] = new ChildToParentRecord(
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

    private static List<IncomingCrossConfigReferenceResult> BuildResults(
        List<IncomingReferenceRecord> incomingRefs,
        Dictionary<Guid, ResourceMetadata> targetMetadata,
        Dictionary<Guid, ChildToParentRecord> childToParent)
    {
        var results = new List<IncomingCrossConfigReferenceResult>();

        foreach (var incoming in incomingRefs)
        {
            if (!targetMetadata.TryGetValue(incoming.TargetResourceId.Value, out var target))
                continue;

            foreach (var (childId, mapping) in childToParent)
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

    private readonly record struct IncomingReferenceRecord(
        InfrastructureConfigId SiblingId,
        string SiblingName,
        Guid ReferenceId,
        AzureResourceId TargetResourceId);

    private readonly record struct ChildToParentRecord(
        Guid ParentId,
        Guid SiblingConfigId,
        string SiblingConfigName,
        string ChildName,
        string ChildType,
        string ChildRgName);
}
