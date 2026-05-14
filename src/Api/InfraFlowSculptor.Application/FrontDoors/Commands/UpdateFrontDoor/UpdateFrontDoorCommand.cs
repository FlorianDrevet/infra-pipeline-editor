using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.FrontDoors.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;

namespace InfraFlowSculptor.Application.FrontDoors.Commands.UpdateFrontDoor;

/// <summary>Updates an existing Front Door resource.</summary>
public record UpdateFrontDoorCommand(
    AzureResourceId Id,
    Name Name,
    Location Location,
    bool WafPolicyEnabled = false,
    IReadOnlyList<FrontDoorEnvironmentConfigData>? EnvironmentSettings = null
) : ICommand<FrontDoorResult>;
