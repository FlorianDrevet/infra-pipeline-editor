using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.ContainerAppEnvironments.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.ContainerAppEnvironments.Commands.UpdateContainerAppEnvironment;

/// <summary>
/// Handles the <see cref="UpdateContainerAppEnvironmentCommand"/> request.
/// </summary>
public sealed class UpdateContainerAppEnvironmentCommandHandler(
    IContainerAppEnvironmentRepository containerAppEnvironmentRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : ICommandHandler<UpdateContainerAppEnvironmentCommand, ContainerAppEnvironmentResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<ContainerAppEnvironmentResult>> Handle(
        UpdateContainerAppEnvironmentCommand request,
        CancellationToken cancellationToken)
    {
        var containerAppEnvironment = await containerAppEnvironmentRepository.GetByIdAsync(request.Id, cancellationToken);
        if (containerAppEnvironment is null)
            return Errors.ContainerAppEnvironment.NotFoundError(request.Id);

        var resourceGroup = await resourceGroupRepository.GetByIdAsync(containerAppEnvironment.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.ContainerAppEnvironment.NotFoundError(request.Id);

        var authResult = await accessService.VerifyWriteAccessAsync(resourceGroup.InfraConfigId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        containerAppEnvironment.Update(request.Name, request.Location);

        if (request.EnvironmentSettings is not null)
            containerAppEnvironment.SetAllEnvironmentSettings(
                request.EnvironmentSettings
                    .Select(ec => (ec.EnvironmentName, ec.Sku, ec.WorkloadProfileType, ec.InternalLoadBalancerEnabled, ec.ZoneRedundancyEnabled, ec.LogAnalyticsWorkspaceId))
                    .ToList());

        var updated = await containerAppEnvironmentRepository.UpdateAsync(containerAppEnvironment);

        return mapper.Map<ContainerAppEnvironmentResult>(updated);
    }
}
