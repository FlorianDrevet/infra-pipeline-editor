namespace InfraFlowSculptor.Application.NetworkSecurityGroups.Common;

/// <summary>Carries NSG rule data within CQRS commands and results.</summary>
public record NsgRuleData(
    string Id,
    string Name,
    int Priority,
    string Direction,
    string Access,
    string Protocol,
    string SourceAddressPrefix,
    string DestinationAddressPrefix,
    string SourcePortRange,
    string DestinationPortRange);
