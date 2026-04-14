using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.ContainerAppEnvironments.Commands.DeleteContainerAppEnvironment;

/// <summary>Command to permanently delete a Container App Environment resource.</summary>
public record DeleteContainerAppEnvironmentCommand(
    AzureResourceId Id
) : ICommand<Deleted>;
