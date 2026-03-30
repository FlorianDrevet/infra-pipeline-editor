using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Application.ContainerApps.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;

namespace InfraFlowSculptor.Application.ContainerApps.Commands.UpdateContainerApp;

/// <summary>Command to update an existing Container App resource.</summary>
public record UpdateContainerAppCommand(
    AzureResourceId Id,
    Name Name,
    Location Location,
    Guid ContainerAppEnvironmentId,
    Guid? ContainerRegistryId,
    IReadOnlyList<ContainerAppEnvironmentConfigData>? EnvironmentSettings = null
) : ICommand<ContainerAppResult>;
