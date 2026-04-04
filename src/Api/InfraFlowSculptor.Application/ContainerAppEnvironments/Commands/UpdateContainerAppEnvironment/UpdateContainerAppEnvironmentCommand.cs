using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Application.ContainerAppEnvironments.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;

namespace InfraFlowSculptor.Application.ContainerAppEnvironments.Commands.UpdateContainerAppEnvironment;

/// <summary>Command to update an existing Container App Environment resource.</summary>
public record UpdateContainerAppEnvironmentCommand(
    AzureResourceId Id,
    Name Name,
    Location Location,
    Guid? LogAnalyticsWorkspaceId = null,
    IReadOnlyList<ContainerAppEnvironmentEnvironmentConfigData>? EnvironmentSettings = null
) : ICommand<ContainerAppEnvironmentResult>;
