using InfraFlowSculptor.Application.ContainerAppEnvironments.Common;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using MediatR;
using ErrorOr;

namespace InfraFlowSculptor.Application.ContainerAppEnvironments.Commands.CreateContainerAppEnvironment;

/// <summary>Command to create a new Container App Environment resource inside a Resource Group.</summary>
public record CreateContainerAppEnvironmentCommand(
    ResourceGroupId ResourceGroupId,
    Name Name,
    Location Location,
    IReadOnlyList<ContainerAppEnvironmentEnvironmentConfigData>? EnvironmentSettings = null
) : IRequest<ErrorOr<ContainerAppEnvironmentResult>>;
