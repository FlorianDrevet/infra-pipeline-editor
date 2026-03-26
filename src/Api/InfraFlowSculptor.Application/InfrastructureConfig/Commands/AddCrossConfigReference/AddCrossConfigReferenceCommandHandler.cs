using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.AddCrossConfigReference;

/// <summary>
/// Handles adding a cross-configuration resource reference.
/// Validates the target resource exists and belongs to a different configuration in the same project.
/// </summary>
public sealed class AddCrossConfigReferenceCommandHandler(
    IInfraConfigAccessService accessService,
    IInfrastructureConfigRepository infraConfigRepository,
    IResourceGroupRepository resourceGroupRepository)
    : IRequestHandler<AddCrossConfigReferenceCommand, ErrorOr<CrossConfigReferenceResult>>
{
    /// <inheritdoc />
    public async Task<ErrorOr<CrossConfigReferenceResult>> Handle(
        AddCrossConfigReferenceCommand command,
        CancellationToken cancellationToken)
    {
        var configId = new InfrastructureConfigId(command.InfraConfigId);
        var authResult = await accessService.VerifyWriteAccessAsync(configId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        var config = authResult.Value;

        // Find which resource group (and hence which config) the target resource belongs to
        var targetResourceId = new AzureResourceId(command.TargetResourceId);
        var targetRg = await resourceGroupRepository.GetByResourceIdAsync(targetResourceId, cancellationToken);
        if (targetRg is null)
            return Domain.Common.Errors.Errors.InfrastructureConfig.TargetResourceNotFound(targetResourceId);

        var targetConfigId = targetRg.InfraConfigId;

        // Verify target config is in the same project
        var targetConfig = await infraConfigRepository.GetByIdAsync(targetConfigId, cancellationToken);
        if (targetConfig is null || targetConfig.ProjectId != config.ProjectId)
            return Domain.Common.Errors.Errors.InfrastructureConfig.TargetResourceNotInSameProject();

        // Add the cross-config reference (domain validates same-config and duplicate)
        var result = config.AddCrossConfigReference(targetConfigId, targetResourceId);
        if (result.IsError)
            return result.Errors;

        await infraConfigRepository.UpdateAsync(config);

        var reference = result.Value;
        return new CrossConfigReferenceResult(
            reference.Id.Value,
            reference.TargetConfigId.Value,
            reference.TargetResourceId.Value);
    }
}
