using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.FrontDoors.Common;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.FrontDoors.Commands.CreateFrontDoor;

/// <summary>Creates a new Front Door resource.</summary>
public record CreateFrontDoorCommand(
    ResourceGroupId ResourceGroupId,
    Name Name,
    Location Location,
    bool WafPolicyEnabled = false,
    IReadOnlyList<FrontDoorEnvironmentConfigData>? EnvironmentSettings = null,
    bool IsExisting = false
) : ICommand<FrontDoorResult>;
