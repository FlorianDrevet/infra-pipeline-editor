namespace InfraFlowSculptor.Contracts.NetworkSecurityGroups.Responses;

/// <summary>Represents an Azure Network Security Group resource.</summary>
public record NetworkSecurityGroupResponse(
    string Id,
    string ResourceGroupId,
    string Name,
    string Location,
    IReadOnlyList<NsgRuleResponse> SecurityRules,
    bool IsExisting = false
);

/// <summary>Response DTO for a security rule in a Network Security Group.</summary>
public record NsgRuleResponse(
    string Id,
    string Name,
    int Priority,
    string Direction,
    string Access,
    string Protocol,
    string SourceAddressPrefix,
    string DestinationAddressPrefix,
    string SourcePortRange,
    string DestinationPortRange
);
