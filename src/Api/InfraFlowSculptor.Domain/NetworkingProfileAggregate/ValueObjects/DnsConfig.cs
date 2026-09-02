using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.NetworkingProfileAggregate.ValueObjects;

/// <summary>
/// Encapsulates DNS configuration for private endpoint resolution.
/// </summary>
public sealed class DnsConfig : ValueObject
{
    /// <summary>Gets the DNS management mode.</summary>
    public DnsMode Mode { get; private set; }

    /// <summary>Gets the resource group ID of the centralized DNS hub (when <see cref="DnsMode.Mode.CentralizedHub"/>).</summary>
    public string? HubResourceGroupId { get; private set; }

    /// <summary>Gets the subscription ID of the centralized DNS hub (when <see cref="DnsMode.Mode.CentralizedHub"/>).</summary>
    public string? HubSubscriptionId { get; private set; }

    private DnsConfig() => Mode = new DnsMode(DnsMode.Mode.AutoManaged);

    /// <summary>Creates an auto-managed DNS configuration.</summary>
    public static DnsConfig AutoManaged()
    {
        return new DnsConfig
        {
            Mode = new DnsMode(DnsMode.Mode.AutoManaged)
        };
    }

    /// <summary>Creates a centralized hub DNS configuration.</summary>
    public static DnsConfig CentralizedHub(string hubResourceGroupId, string hubSubscriptionId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(hubResourceGroupId);
        ArgumentException.ThrowIfNullOrWhiteSpace(hubSubscriptionId);

        return new DnsConfig
        {
            Mode = new DnsMode(DnsMode.Mode.CentralizedHub),
            HubResourceGroupId = hubResourceGroupId,
            HubSubscriptionId = hubSubscriptionId
        };
    }

    /// <summary>Creates a custom DNS configuration.</summary>
    public static DnsConfig Custom()
    {
        return new DnsConfig
        {
            Mode = new DnsMode(DnsMode.Mode.Custom)
        };
    }

    /// <inheritdoc />
    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Mode;
        yield return HubResourceGroupId ?? string.Empty;
        yield return HubSubscriptionId ?? string.Empty;
    }
}
