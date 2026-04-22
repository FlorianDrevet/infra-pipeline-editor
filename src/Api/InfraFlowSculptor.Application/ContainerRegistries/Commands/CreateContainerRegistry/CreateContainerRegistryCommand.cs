using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.ContainerRegistries.Common;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.ContainerRegistries.Commands.CreateContainerRegistry;

/// <summary>Command to create a new Container Registry resource inside a Resource Group.</summary>
public record CreateContainerRegistryCommand(
    ResourceGroupId ResourceGroupId,
    Name Name,
    Location Location,
    IReadOnlyList<ContainerRegistryEnvironmentConfigData>? EnvironmentSettings = null,
    bool IsExisting = false
) : ICommand<ContainerRegistryResult>;
