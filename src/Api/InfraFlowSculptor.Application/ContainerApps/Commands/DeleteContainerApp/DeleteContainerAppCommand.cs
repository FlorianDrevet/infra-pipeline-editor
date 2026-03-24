using ErrorOr;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.ContainerApps.Commands.DeleteContainerApp;

/// <summary>Command to permanently delete a Container App resource.</summary>
public record DeleteContainerAppCommand(
    AzureResourceId Id
) : IRequest<ErrorOr<Deleted>>;
