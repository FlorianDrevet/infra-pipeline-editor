using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;

namespace InfraFlowSculptor.Domain.Common.OwnedEntities;

/// <summary>
/// Resource-level configuration used to deploy a private endpoint for an Azure resource.
/// </summary>
public sealed class PrivateEndpointConfiguration
{
    private const string ExistingHubResourceGroupRequiredMessage = "DNS hub resource group ID is required when using ExistingHub DNS mode.";
    private const string ExistingHubSubscriptionRequiredMessage = "DNS hub subscription ID is required when using ExistingHub DNS mode.";

    private PrivateEndpointConfiguration()
    {
    }

    private PrivateEndpointConfiguration(
        AzureResourceId virtualNetworkId,
        Name subnetName,
        PrivateEndpointDnsMode dnsMode,
        string? dnsHubResourceGroupId,
        string? dnsHubSubscriptionId)
    {
        ArgumentNullException.ThrowIfNull(virtualNetworkId);
        ArgumentNullException.ThrowIfNull(subnetName);
        ArgumentNullException.ThrowIfNull(dnsMode);

        ValidateDnsHubConfiguration(dnsMode, dnsHubResourceGroupId, dnsHubSubscriptionId);

        VirtualNetworkId = virtualNetworkId;
        SubnetName = subnetName;
        DnsMode = dnsMode;
        DnsHubResourceGroupId = dnsMode.Value == PrivateEndpointDnsMode.Mode.ExistingHub
            ? dnsHubResourceGroupId!.Trim()
            : null;
        DnsHubSubscriptionId = dnsMode.Value == PrivateEndpointDnsMode.Mode.ExistingHub
            ? dnsHubSubscriptionId!.Trim()
            : null;
    }

    /// <summary>
    /// Gets the virtual network selected for the private endpoint.
    /// </summary>
    public AzureResourceId VirtualNetworkId { get; private set; } = null!;

    /// <summary>
    /// Gets the subnet name selected for the private endpoint.
    /// </summary>
    public Name SubnetName { get; private set; } = null!;

    /// <summary>
    /// Gets the DNS management mode for the private endpoint.
    /// </summary>
    public PrivateEndpointDnsMode DnsMode { get; private set; } = null!;

    /// <summary>
    /// Gets the centralized DNS hub resource group identifier when using an existing hub.
    /// </summary>
    public string? DnsHubResourceGroupId { get; private set; }

    /// <summary>
    /// Gets the centralized DNS hub subscription identifier when using an existing hub.
    /// </summary>
    public string? DnsHubSubscriptionId { get; private set; }

    /// <summary>
    /// Creates a configuration where InfraFlowSculptor manages Private DNS Zones and VNet links.
    /// </summary>
    /// <param name="virtualNetworkId">The selected virtual network identifier.</param>
    /// <param name="subnetName">The selected subnet name.</param>
    /// <returns>The created private endpoint configuration.</returns>
    public static PrivateEndpointConfiguration AutoManaged(AzureResourceId virtualNetworkId, Name subnetName)
    {
        return new PrivateEndpointConfiguration(
            virtualNetworkId,
            subnetName,
            new PrivateEndpointDnsMode(PrivateEndpointDnsMode.Mode.AutoManaged),
            dnsHubResourceGroupId: null,
            dnsHubSubscriptionId: null);
    }

    /// <summary>
    /// Creates a configuration that targets an existing centralized DNS hub.
    /// </summary>
    /// <param name="virtualNetworkId">The selected virtual network identifier.</param>
    /// <param name="subnetName">The selected subnet name.</param>
    /// <param name="dnsHubResourceGroupId">The centralized DNS hub resource group identifier.</param>
    /// <param name="dnsHubSubscriptionId">The centralized DNS hub subscription identifier.</param>
    /// <returns>The created private endpoint configuration.</returns>
    public static PrivateEndpointConfiguration ExistingHub(
        AzureResourceId virtualNetworkId,
        Name subnetName,
        string dnsHubResourceGroupId,
        string dnsHubSubscriptionId)
    {
        return new PrivateEndpointConfiguration(
            virtualNetworkId,
            subnetName,
            new PrivateEndpointDnsMode(PrivateEndpointDnsMode.Mode.ExistingHub),
            dnsHubResourceGroupId,
            dnsHubSubscriptionId);
    }

    /// <summary>
    /// Creates a configuration where DNS is managed outside InfraFlowSculptor.
    /// </summary>
    /// <param name="virtualNetworkId">The selected virtual network identifier.</param>
    /// <param name="subnetName">The selected subnet name.</param>
    /// <returns>The created private endpoint configuration.</returns>
    public static PrivateEndpointConfiguration Disabled(AzureResourceId virtualNetworkId, Name subnetName)
    {
        return new PrivateEndpointConfiguration(
            virtualNetworkId,
            subnetName,
            new PrivateEndpointDnsMode(PrivateEndpointDnsMode.Mode.Disabled),
            dnsHubResourceGroupId: null,
            dnsHubSubscriptionId: null);
    }

    private static void ValidateDnsHubConfiguration(
        PrivateEndpointDnsMode dnsMode,
        string? dnsHubResourceGroupId,
        string? dnsHubSubscriptionId)
    {
        if (dnsMode.Value != PrivateEndpointDnsMode.Mode.ExistingHub)
            return;

        if (string.IsNullOrWhiteSpace(dnsHubResourceGroupId))
            throw new ArgumentException(ExistingHubResourceGroupRequiredMessage, nameof(dnsHubResourceGroupId));

        if (string.IsNullOrWhiteSpace(dnsHubSubscriptionId))
            throw new ArgumentException(ExistingHubSubscriptionRequiredMessage, nameof(dnsHubSubscriptionId));
    }
}