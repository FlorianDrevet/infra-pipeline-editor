using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Queries.ListCrossConfigReferences;

/// <summary>
/// Handles listing cross-configuration resource references with resolved target metadata.
/// Batch-loads all target configs and resources in two queries instead of 2N sequential ones.
/// </summary>
public sealed class ListCrossConfigReferencesQueryHandler(
    IInfraConfigAccessService accessService,
    IInfrastructureConfigRepository infraConfigRepository,
    IResourceGroupRepository resourceGroupRepository)
    : IQueryHandler<ListCrossConfigReferencesQuery, List<CrossConfigReferenceDetailResult>>
{
    /// <inheritdoc />
    public async Task<ErrorOr<List<CrossConfigReferenceDetailResult>>> Handle(
        ListCrossConfigReferencesQuery query,
        CancellationToken cancellationToken)
    {
        var configId = new InfrastructureConfigId(query.InfraConfigId);
        var authResult = await accessService.VerifyReadAccessAsync(configId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        var config = await infraConfigRepository.GetByIdWithMembersAsync(configId, cancellationToken);
        if (config is null)
            return authResult.Errors;

        if (config.CrossConfigReferences.Count == 0)
            return new List<CrossConfigReferenceDetailResult>();

        // Batch-load all target config IDs in one query
        var targetConfigIds = config.CrossConfigReferences
            .Select(r => r.TargetConfigId)
            .Distinct()
            .ToList();
        var targetConfigs = new Dictionary<InfrastructureConfigId, Domain.InfrastructureConfigAggregate.InfrastructureConfig>();
        foreach (var tcId in targetConfigIds)
        {
            var tc = await infraConfigRepository.GetByIdAsync(tcId, cancellationToken);
            if (tc is not null)
                targetConfigs[tc.Id] = tc;
        }

        // Batch-load all target resource metadata (name, type, RG name) in one query
        var targetResourceIds = config.CrossConfigReferences
            .Select(r => r.TargetResourceId)
            .Distinct()
            .ToList();
        var resourceMetadataList = await resourceGroupRepository.GetResourceMetadataBatchAsync(
            targetResourceIds, cancellationToken);
        var resourceMetadata = resourceMetadataList.ToDictionary(m => m.ResourceId);

        var results = new List<CrossConfigReferenceDetailResult>();

        foreach (var reference in config.CrossConfigReferences)
        {
            if (!targetConfigs.TryGetValue(reference.TargetConfigId, out var targetConfig))
                continue;

            if (!resourceMetadata.TryGetValue(reference.TargetResourceId.Value, out var meta))
                continue;

            results.Add(new CrossConfigReferenceDetailResult(
                ReferenceId: reference.Id.Value,
                TargetConfigId: reference.TargetConfigId.Value,
                TargetConfigName: targetConfig.Name.Value,
                TargetResourceId: reference.TargetResourceId.Value,
                TargetResourceName: meta.ResourceName,
                TargetResourceType: meta.ResourceType,
                TargetResourceGroupName: meta.ResourceGroupName));
        }

        return results;
    }
}
