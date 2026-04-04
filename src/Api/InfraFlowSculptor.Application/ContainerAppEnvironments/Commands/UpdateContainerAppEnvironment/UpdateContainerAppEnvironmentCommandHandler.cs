using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.ContainerAppEnvironments.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
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
    ILogAnalyticsWorkspaceRepository logAnalyticsWorkspaceRepository,
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

        // Verify the Log Analytics Workspace exists if provided
        AzureResourceId? logAnalyticsWorkspaceId = null;
        if (request.LogAnalyticsWorkspaceId is not null)
        {
            logAnalyticsWorkspaceId = new AzureResourceId(request.LogAnalyticsWorkspaceId.Value);
            var logAnalyticsWorkspace = await logAnalyticsWorkspaceRepository.GetByIdAsync(logAnalyticsWorkspaceId, cancellationToken);
            if (logAnalyticsWorkspace is null)
                return Errors.LogAnalyticsWorkspace.NotFoundError(logAnalyticsWorkspaceId);
        }

        containerAppEnvironment.Update(request.Name, request.Location, logAnalyticsWorkspaceId);

        if (request.EnvironmentSettings is not null)
            containerAppEnvironment.SetAllEnvironmentSettings(
                request.EnvironmentSettings
                    .Select(ec => (ec.EnvironmentName, ec.Sku, ec.WorkloadProfileType, ec.InternalLoadBalancerEnabled, ec.ZoneRedundancyEnabled))
                    .ToList());

        var updated = await containerAppEnvironmentRepository.UpdateAsync(containerAppEnvironment);

        return mapper.Map<ContainerAppEnvironmentResult>(updated);
    }
}
