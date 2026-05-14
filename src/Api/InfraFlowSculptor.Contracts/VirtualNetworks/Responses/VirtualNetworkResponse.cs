using InfraFlowSculptor.Contracts.VirtualNetworks.Requests;

namespace InfraFlowSculptor.Contracts.VirtualNetworks.Responses;

/// <summary>Represents an Azure Virtual Network resource.</summary>
/// <param name="Id">Unique identifier of the Virtual Network.</param>
/// <param name="ResourceGroupId">Identifier of the parent Resource Group.</param>
/// <param name="Name">Display name of the Virtual Network.</param>
/// <param name="Location">Azure region where the Virtual Network is deployed.</param>
/// <param name="EnableDdosProtection">Whether Azure DDoS Protection Standard is enabled.</param>
/// <param name="Subnets">Subnets configured in this virtual network.</param>
/// <param name="EnvironmentSettings">Per-environment typed configuration overrides.</param>
/// <param name="IsExisting">Whether the resource references an already-existing Azure resource.</param>
public record VirtualNetworkResponse(
    string Id,
    string ResourceGroupId,
    string Name,
    string Location,
    bool EnableDdosProtection,
    IReadOnlyList<SubnetResponse> Subnets,
    IReadOnlyList<VirtualNetworkEnvironmentConfigResponse> EnvironmentSettings,
    bool IsExisting = false
);

/// <summary>Response DTO for a subnet within a Virtual Network.</summary>
public record SubnetResponse(
    string Id,
    string Name,
    string? Delegation,
    IReadOnlyList<string>? ServiceEndpoints,
    string PrivateEndpointNetworkPolicies,
    string? NsgId
);

/// <summary>Response DTO for a typed per-environment Virtual Network configuration.</summary>
public record VirtualNetworkEnvironmentConfigResponse(
    string EnvironmentName,
    IReadOnlyList<string> AddressSpaces,
    IReadOnlyList<string>? DnsServers
);
