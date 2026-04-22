using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.ContainerAppEnvironments.Common;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using ErrorOr;

namespace InfraFlowSculptor.Application.ContainerAppEnvironments.Commands.CreateContainerAppEnvironment;

/// <summary>Command to create a new Container App Environment resource inside a Resource Group.</summary>
public record CreateContainerAppEnvironmentCommand(
    ResourceGroupId ResourceGroupId,
    Name Name,
    Location Location,
    Guid? LogAnalyticsWorkspaceId = null,
    IReadOnlyList<ContainerAppEnvironmentEnvironmentConfigData>? EnvironmentSettings = null,
    bool IsExisting = false
) : ICommand<ContainerAppEnvironmentResult>;
