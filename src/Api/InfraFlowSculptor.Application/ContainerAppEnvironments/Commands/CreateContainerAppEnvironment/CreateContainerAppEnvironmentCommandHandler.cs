using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.ContainerAppEnvironments.Common;
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

        var containerAppEnvironment = ContainerAppEnvironment.Create(
            request.ResourceGroupId,
            request.Name,
            request.Location,
            request.EnvironmentSettings?
                .Select(ec => (ec.EnvironmentName, ec.Sku, ec.WorkloadProfileType, ec.InternalLoadBalancerEnabled, ec.ZoneRedundancyEnabled, ec.LogAnalyticsWorkspaceId))
                .ToList());

        var saved = await containerAppEnvironmentRepository.AddAsync(containerAppEnvironment);

        return mapper.Map<ContainerAppEnvironmentResult>(saved);
    }
}
