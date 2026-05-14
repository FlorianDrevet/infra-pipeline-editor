using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.NetworkSecurityGroups.Common;

/// <summary>Application-layer result for a Network Security Group.</summary>
public record NetworkSecurityGroupResult(
    AzureResourceId Id,
    ResourceGroupId ResourceGroupId,
    Name Name,
    Location Location,
    IReadOnlyList<NsgRuleData> SecurityRules,
    bool IsExisting = false
);
