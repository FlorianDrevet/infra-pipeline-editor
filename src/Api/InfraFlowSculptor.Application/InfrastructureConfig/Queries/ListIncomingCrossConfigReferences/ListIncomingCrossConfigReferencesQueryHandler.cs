using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Queries.ListIncomingCrossConfigReferences;

/// <summary>
/// Handles listing incoming cross-config references: resources in OTHER configurations
/// that depend on resources in THIS configuration through cross-config references.
/// For each incoming reference, resolves the child resources in the source configuration
/// whose parent FK matches the referenced target resource.
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

        // Load all sibling configs in the same project
        var siblingConfigs = await infraConfigRepository.GetByProjectIdAsync(config.ProjectId, cancellationToken);

        var results = new List<IncomingCrossConfigReferenceResult>();

        foreach (var sibling in siblingConfigs)
        {
            if (sibling.Id == config.Id) continue;

            // Load sibling with cross-config references
            var siblingWithRefs = await infraConfigRepository.GetByIdWithMembersAsync(sibling.Id, cancellationToken);
            if (siblingWithRefs is null) continue;

            // Find refs that target this config
            var incomingRefs = siblingWithRefs.CrossConfigReferences
                .Where(r => r.TargetConfigId == config.Id)
                .ToList();

            if (incomingRefs.Count == 0) continue;

            // Load source config's resource groups to resolve child→parent mappings
            var sourceRgs = await resourceGroupRepository.GetByInfraConfigIdAsync(sibling.Id, cancellationToken);

            foreach (var ccRef in incomingRefs)
            {
                // Resolve target resource in this config
                var targetRg = await resourceGroupRepository.GetByContainedResourceIdAsync(ccRef.TargetResourceId, cancellationToken);
                if (targetRg is null) continue;

                var targetResource = targetRg.Resources.FirstOrDefault(r => r.Id == ccRef.TargetResourceId);
                if (targetResource is null) continue;

                // Find child resources in the source config whose parent FK is the target resource
                foreach (var sourceRg in sourceRgs)
                {
                    var parentMapping = await resourceGroupRepository.GetChildToParentMappingAsync(sourceRg.Id, cancellationToken);

                    foreach (var (childId, parentId) in parentMapping)
                    {
                        if (parentId != ccRef.TargetResourceId.Value) continue;

                        var childResource = sourceRg.Resources
                            .FirstOrDefault(r => r.Id.Value == childId);
                        if (childResource is null) continue;

                        results.Add(new IncomingCrossConfigReferenceResult(
                            ReferenceId: ccRef.Id.Value,
                            SourceConfigId: sibling.Id.Value,
                            SourceConfigName: siblingWithRefs.Name.Value,
                            SourceResourceId: childResource.Id.Value,
                            SourceResourceName: childResource.Name.Value,
                            SourceResourceType: childResource.GetType().Name,
                            SourceResourceGroupName: sourceRg.Name.Value,
                            TargetResourceId: targetResource.Id.Value,
                            TargetResourceName: targetResource.Name.Value,
                            TargetResourceType: targetResource.GetType().Name));
                    }
                }
            }
        }

        return results;
    }
}
