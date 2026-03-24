using ErrorOr;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.ContainerAppEnvironments.Commands.DeleteContainerAppEnvironment;

/// <summary>Command to permanently delete a Container App Environment resource.</summary>
public record DeleteContainerAppEnvironmentCommand(
    AzureResourceId Id
) : IRequest<ErrorOr<Deleted>>;
