namespace InfraFlowSculptor.GenerationCore.Models;

/// <summary>
/// Represents a subnet definition for Bicep generation.
/// </summary>
public class SubnetDefinition
{
    /// <summary>Gets or sets the subnet name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the address prefix in CIDR notation (e.g. 10.0.1.0/24).</summary>
    public string AddressPrefix { get; set; } = string.Empty;

    /// <summary>Gets or sets the optional subnet delegation ARM service name.</summary>
    public string? Delegation { get; set; }

    /// <summary>Gets or sets the optional service endpoints.</summary>
    public IReadOnlyList<string>? ServiceEndpoints { get; set; }

    /// <summary>Gets or sets the private endpoint network policies mode.</summary>
    public string PrivateEndpointNetworkPolicies { get; set; } = "Disabled";

    /// <summary>Gets or sets an optional NSG resource ID reference.</summary>
    public string? NsgId { get; set; }
}
