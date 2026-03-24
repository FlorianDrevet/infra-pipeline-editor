using ErrorOr;
using InfraFlowSculptor.Application.ContainerApps.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.ContainerApps.Commands.UpdateContainerApp;

/// <summary>Command to update an existing Container App resource.</summary>
public record UpdateContainerAppCommand(
    AzureResourceId Id,
    Name Name,
    Location Location,
    Guid ContainerAppEnvironmentId,
    IReadOnlyList<ContainerAppEnvironmentConfigData>? EnvironmentSettings = null
) : IRequest<ErrorOr<ContainerAppResult>>;
