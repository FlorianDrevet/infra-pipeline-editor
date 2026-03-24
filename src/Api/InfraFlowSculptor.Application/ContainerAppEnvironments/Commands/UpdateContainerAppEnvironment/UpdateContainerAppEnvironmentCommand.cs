using ErrorOr;
using InfraFlowSculptor.Application.ContainerAppEnvironments.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.ContainerAppEnvironments.Commands.UpdateContainerAppEnvironment;

/// <summary>Command to update an existing Container App Environment resource.</summary>
public record UpdateContainerAppEnvironmentCommand(
    AzureResourceId Id,
    Name Name,
    Location Location,
    IReadOnlyList<ContainerAppEnvironmentEnvironmentConfigData>? EnvironmentSettings = null
) : IRequest<ErrorOr<ContainerAppEnvironmentResult>>;
