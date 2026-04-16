using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.ContainerRegistries.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;

namespace InfraFlowSculptor.Application.ContainerRegistries.Commands.UpdateContainerRegistry;

/// <summary>Command to update an existing Container Registry resource.</summary>
public record UpdateContainerRegistryCommand(
    AzureResourceId Id,
    Name Name,
    Location Location,
    IReadOnlyCollection<ContainerRegistryEnvironmentConfigData>? EnvironmentSettings = null
) : ICommand<ContainerRegistryResult>;
