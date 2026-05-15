using InfraFlowSculptor.Domain.NetworkSecurityGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.NetworkSecurityGroupAggregate;

/// <summary>Groups all properties of an NSG security rule for creation or update.</summary>
public sealed record NsgRuleParameters(
    string Name,
    int Priority,
    NsgDirection Direction,
    NsgAccess Access,
    NsgProtocol Protocol,
    string SourceAddressPrefix,
    string DestinationAddressPrefix,
    string SourcePortRange,
    string DestinationPortRange);
