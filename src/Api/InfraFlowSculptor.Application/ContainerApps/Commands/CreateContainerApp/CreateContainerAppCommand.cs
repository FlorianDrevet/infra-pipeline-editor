using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.ContainerApps.Common;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using ErrorOr;

namespace InfraFlowSculptor.Application.ContainerApps.Commands.CreateContainerApp;

/// <summary>Command to create a new Container App inside a Resource Group.</summary>
public record CreateContainerAppCommand(
    ResourceGroupId ResourceGroupId,
    Name Name,
    Location Location,
    Guid ContainerAppEnvironmentId,
    Guid? ContainerRegistryId,
    IReadOnlyList<ContainerAppEnvironmentConfigData>? EnvironmentSettings = null
) : ICommand<ContainerAppResult>;
