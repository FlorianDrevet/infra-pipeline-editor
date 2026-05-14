using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.NetworkSecurityGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.NetworkSecurityGroupAggregate.Entities;

/// <summary>Represents a security rule in a Network Security Group.</summary>
public sealed class NsgRule : Entity<NsgRuleId>
{
    /// <summary>Gets the parent NSG identifier.</summary>
    public AzureResourceId NetworkSecurityGroupId { get; private set; } = null!;

    /// <summary>Gets the rule name.</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Gets the rule priority (100-4096).</summary>
    public int Priority { get; private set; }

    /// <summary>Gets the traffic direction.</summary>
    public NsgDirection Direction { get; private set; } = null!;

    /// <summary>Gets the access action.</summary>
    public NsgAccess Access { get; private set; } = null!;

    /// <summary>Gets the network protocol.</summary>
    public NsgProtocol Protocol { get; private set; } = null!;

    /// <summary>Gets the source address prefix or CIDR.</summary>
    public string SourceAddressPrefix { get; private set; } = string.Empty;

    /// <summary>Gets the destination address prefix or CIDR.</summary>
    public string DestinationAddressPrefix { get; private set; } = string.Empty;

    /// <summary>Gets the source port range.</summary>
    public string SourcePortRange { get; private set; } = string.Empty;

    /// <summary>Gets the destination port range.</summary>
    public string DestinationPortRange { get; private set; } = string.Empty;

    private NsgRule() { }

    /// <summary>Updates rule properties.</summary>
    public void Update(
        string name, int priority, NsgDirection direction, NsgAccess access, NsgProtocol protocol,
        string sourceAddressPrefix, string destinationAddressPrefix, string sourcePortRange, string destinationPortRange)
    {
        Name = name;
        Priority = priority;
        Direction = direction;
        Access = access;
        Protocol = protocol;
        SourceAddressPrefix = sourceAddressPrefix;
        DestinationAddressPrefix = destinationAddressPrefix;
        SourcePortRange = sourcePortRange;
        DestinationPortRange = destinationPortRange;
    }

    /// <summary>Creates a new NSG rule.</summary>
    internal static NsgRule Create(
        AzureResourceId nsgId, string name, int priority, NsgDirection direction, NsgAccess access, NsgProtocol protocol,
        string sourceAddressPrefix, string destinationAddressPrefix, string sourcePortRange, string destinationPortRange)
    {
        return new NsgRule
        {
            Id = NsgRuleId.CreateUnique(),
            NetworkSecurityGroupId = nsgId,
            Name = name,
            Priority = priority,
            Direction = direction,
            Access = access,
            Protocol = protocol,
            SourceAddressPrefix = sourceAddressPrefix,
            DestinationAddressPrefix = destinationAddressPrefix,
            SourcePortRange = sourcePortRange,
            DestinationPortRange = destinationPortRange
        };
    }
}
