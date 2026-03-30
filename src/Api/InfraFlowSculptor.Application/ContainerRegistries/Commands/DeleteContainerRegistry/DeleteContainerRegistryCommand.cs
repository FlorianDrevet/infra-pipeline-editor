using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using ErrorOr;

namespace InfraFlowSculptor.Application.ContainerRegistries.Commands.DeleteContainerRegistry;

/// <summary>Command to permanently delete a Container Registry resource.</summary>
public record DeleteContainerRegistryCommand(
    AzureResourceId Id
) : ICommand<Deleted>;
