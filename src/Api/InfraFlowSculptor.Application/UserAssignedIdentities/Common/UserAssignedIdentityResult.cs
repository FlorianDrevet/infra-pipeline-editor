using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.UserAssignedIdentities.Common;

/// <summary>
/// Application-layer result model for a user-assigned identity resource.
/// </summary>
public record UserAssignedIdentityResult(
    AzureResourceId Id,
    ResourceGroupId ResourceGroupId,
    Name Name,
    Location Location
);
