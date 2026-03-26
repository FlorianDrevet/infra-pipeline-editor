using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Queries.ListCrossConfigReferences;

/// <summary>
/// Handles listing cross-configuration resource references with resolved target metadata.
/// </summary>
public sealed class ListCrossConfigReferencesQueryHandler(
    IInfraConfigAccessService accessService,
    IInfrastructureConfigRepository infraConfigRepository,
    IResourceGroupRepository resourceGroupRepository)
    : IRequestHandler<ListCrossConfigReferencesQuery, ErrorOr<List<CrossConfigReferenceDetailResult>>>
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

        var config = authResult.Value;
        var results = new List<CrossConfigReferenceDetailResult>();

        foreach (var reference in config.CrossConfigReferences)
        {
            var targetConfig = await infraConfigRepository.GetByIdAsync(reference.TargetConfigId, cancellationToken);
            if (targetConfig is null) continue;

            var targetRg = await resourceGroupRepository.GetByResourceIdAsync(
                reference.TargetResourceId, cancellationToken);
            if (targetRg is null) continue;

            var targetResource = targetRg.Resources.FirstOrDefault(r => r.Id == reference.TargetResourceId);
            if (targetResource is null) continue;

            results.Add(new CrossConfigReferenceDetailResult(
                ReferenceId: reference.Id.Value,
                TargetConfigId: reference.TargetConfigId.Value,
                TargetConfigName: targetConfig.Name.Value,
                TargetResourceId: reference.TargetResourceId.Value,
                TargetResourceName: targetResource.Name.Value,
                TargetResourceType: targetResource.GetType().Name,
                TargetResourceGroupName: targetRg.Name.Value));
        }

        return results;
    }
}
