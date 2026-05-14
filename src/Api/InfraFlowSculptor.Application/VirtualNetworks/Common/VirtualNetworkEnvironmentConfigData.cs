namespace InfraFlowSculptor.Application.VirtualNetworks.Common;

/// <summary>Carries typed per-environment Virtual Network configuration data within CQRS commands and results.</summary>
/// <param name="EnvironmentName">Name of the environment.</param>
/// <param name="AddressSpaces">Address spaces in CIDR notation.</param>
/// <param name="DnsServers">Optional custom DNS servers.</param>
public record VirtualNetworkEnvironmentConfigData(
    string EnvironmentName,
    IReadOnlyList<string> AddressSpaces,
    IReadOnlyList<string>? DnsServers);
