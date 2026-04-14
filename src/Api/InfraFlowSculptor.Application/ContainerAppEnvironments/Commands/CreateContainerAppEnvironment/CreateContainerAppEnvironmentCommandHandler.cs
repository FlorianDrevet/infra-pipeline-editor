using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.ContainerAppEnvironments.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.ContainerAppEnvironmentAggregate;
using MapsterMapper;
using MediatR;
using ErrorOr;

namespace InfraFlowSculptor.Application.ContainerAppEnvironments.Commands.CreateContainerAppEnvironment;

/// <summary>
/// Handles the <see cref="CreateContainerAppEnvironmentCommand"/> request.
/// </summary>
public sealed class CreateContainerAppEnvironmentCommandHandler(
    IContainerAppEnvironmentRepository containerAppEnvironmentRepository,
    IResourceGroupRepository resourceGroupRepository,
    ILogAnalyticsWorkspaceRepository logAnalyticsWorkspaceRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : ICommandHandler<CreateContainerAppEnvironmentCommand, ContainerAppEnvironmentResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<ContainerAppEnvironmentResult>> Handle(
        CreateContainerAppEnvironmentCommand request,
        CancellationToken cancellationToken)
    {
        var resourceGroup = await resourceGroupRepository.GetByIdAsync(request.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.ResourceGroup.NotFound(request.ResourceGroupId);

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

        var containerAppEnvironment = ContainerAppEnvironment.Create(
            request.ResourceGroupId,
            request.Name,
            request.Location,
            logAnalyticsWorkspaceId,
            request.EnvironmentSettings?
                .Select(ec => (ec.EnvironmentName, ec.Sku, ec.WorkloadProfileType, ec.InternalLoadBalancerEnabled, ec.ZoneRedundancyEnabled))
                .ToList());

        var saved = await containerAppEnvironmentRepository.AddAsync(containerAppEnvironment);

        return mapper.Map<ContainerAppEnvironmentResult>(saved);
    }
}
