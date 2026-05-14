using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.NetworkSecurityGroupAggregate.Entities;
using InfraFlowSculptor.Domain.NetworkSecurityGroupAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.NetworkSecurityGroupAggregate;

/// <summary>
/// Represents an Azure Network Security Group (<c>Microsoft.Network/networkSecurityGroups</c>).
/// Contains inbound and outbound security rules to filter network traffic.
/// </summary>
public sealed class NetworkSecurityGroup : AzureResource
{
    private readonly List<NsgRule> _securityRules = [];
    /// <summary>Gets the security rules defined in this NSG.</summary>
    public IReadOnlyCollection<NsgRule> SecurityRules => _securityRules.AsReadOnly();

    private NetworkSecurityGroup() { }

    /// <summary>Updates the resource-level properties.</summary>
    public void Update(Name name, Location location)
    {
        SetNameAndLocation(name, location);
    }

    /// <summary>Adds a security rule to this NSG.</summary>
    public NsgRule AddRule(
        string name,
        int priority,
        NsgDirection direction,
        NsgAccess access,
        NsgProtocol protocol,
        string sourceAddressPrefix,
        string destinationAddressPrefix,
        string sourcePortRange,
        string destinationPortRange)
    {
        if (_securityRules.Any(r => r.Name == name))
            throw new InvalidOperationException($"A rule named '{name}' already exists in this NSG.");
        if (_securityRules.Any(r => r.Priority == priority && r.Direction.Value == direction.Value))
            throw new InvalidOperationException($"A rule with priority {priority} already exists for direction {direction.Value}.");

        var rule = NsgRule.Create(Id, name, priority, direction, access, protocol, sourceAddressPrefix, destinationAddressPrefix, sourcePortRange, destinationPortRange);
        _securityRules.Add(rule);
        return rule;
    }

    /// <summary>Removes a security rule by its identifier.</summary>
    public void RemoveRule(NsgRuleId ruleId)
    {
        var rule = _securityRules.FirstOrDefault(r => r.Id == ruleId)
            ?? throw new InvalidOperationException($"Rule '{ruleId.Value}' not found.");
        _securityRules.Remove(rule);
    }

    /// <summary>Updates an existing security rule.</summary>
    public void UpdateRule(
        NsgRuleId ruleId,
        string name,
        int priority,
        NsgDirection direction,
        NsgAccess access,
        NsgProtocol protocol,
        string sourceAddressPrefix,
        string destinationAddressPrefix,
        string sourcePortRange,
        string destinationPortRange)
    {
        var rule = _securityRules.FirstOrDefault(r => r.Id == ruleId)
            ?? throw new InvalidOperationException($"Rule '{ruleId.Value}' not found.");
        rule.Update(name, priority, direction, access, protocol, sourceAddressPrefix, destinationAddressPrefix, sourcePortRange, destinationPortRange);
    }

    /// <summary>Creates a new NetworkSecurityGroup.</summary>
    public static NetworkSecurityGroup Create(
        ResourceGroupId resourceGroupId,
        Name name,
        Location location,
        bool isExisting = false)
    {
        return new NetworkSecurityGroup
        {
            Id = AzureResourceId.CreateUnique(),
            ResourceGroupId = resourceGroupId,
            Name = name,
            Location = location,
            IsExisting = isExisting
        };
    }
}
