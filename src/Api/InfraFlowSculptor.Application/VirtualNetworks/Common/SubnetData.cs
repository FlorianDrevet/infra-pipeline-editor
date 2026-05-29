namespace InfraFlowSculptor.Application.VirtualNetworks.Common;

/// <summary>Carries subnet data within CQRS commands and results.</summary>
/// <param name="Id">Subnet identifier.</param>
/// <param name="Name">Subnet name.</param>
/// <param name="AddressPrefix">Address prefix in CIDR notation.</param>
/// <param name="Delegation">Optional subnet delegation.</param>
/// <param name="ServiceEndpoints">Optional service endpoints.</param>
/// <param name="PrivateEndpointNetworkPolicies">Private endpoint network policy mode.</param>
/// <param name="NsgId">Optional reference to a Network Security Group.</param>
public record SubnetData(
    string Id,
    string Name,
    string AddressPrefix,
    string? Delegation,
    IReadOnlyList<string>? ServiceEndpoints,
    string PrivateEndpointNetworkPolicies,
    string? NsgId);
