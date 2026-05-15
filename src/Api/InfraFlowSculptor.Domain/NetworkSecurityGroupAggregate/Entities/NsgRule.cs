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
    public void Update(NsgRuleParameters data)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(data.Name);
        ArgumentOutOfRangeException.ThrowIfLessThan(data.Priority, 100);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(data.Priority, 4096);

        Name = data.Name;
        Priority = data.Priority;
        Direction = data.Direction;
        Access = data.Access;
        Protocol = data.Protocol;
        SourceAddressPrefix = data.SourceAddressPrefix;
        DestinationAddressPrefix = data.DestinationAddressPrefix;
        SourcePortRange = data.SourcePortRange;
        DestinationPortRange = data.DestinationPortRange;
    }

    /// <summary>Creates a new NSG rule.</summary>
    internal static NsgRule Create(AzureResourceId nsgId, NsgRuleParameters data)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(data.Name);
        ArgumentOutOfRangeException.ThrowIfLessThan(data.Priority, 100);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(data.Priority, 4096);

        return new NsgRule
        {
            Id = NsgRuleId.CreateUnique(),
            NetworkSecurityGroupId = nsgId,
            Name = data.Name,
            Priority = data.Priority,
            Direction = data.Direction,
            Access = data.Access,
            Protocol = data.Protocol,
            SourceAddressPrefix = data.SourceAddressPrefix,
            DestinationAddressPrefix = data.DestinationAddressPrefix,
            SourcePortRange = data.SourcePortRange,
            DestinationPortRange = data.DestinationPortRange
        };
    }
}
