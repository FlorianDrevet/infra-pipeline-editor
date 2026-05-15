using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.FrontDoors.Common;

/// <summary>Application-layer result for a Front Door.</summary>
public record FrontDoorResult(
    AzureResourceId Id,
    ResourceGroupId ResourceGroupId,
    Name Name,
    Location Location,
    bool WafPolicyEnabled,
    IReadOnlyList<FrontDoorOriginData> Origins,
    IReadOnlyList<FrontDoorEnvironmentConfigData> EnvironmentSettings,
    bool IsExisting = false
);
