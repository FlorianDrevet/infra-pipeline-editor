using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.ContainerApps.Commands.DeleteContainerApp;

/// <summary>Command to permanently delete a Container App resource.</summary>
public record DeleteContainerAppCommand(
    AzureResourceId Id
) : ICommand<Deleted>;
