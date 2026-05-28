using ErrorOr;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using InfraFlowSculptor.Domain.NetworkingProfileAggregate.Entities;
using InfraFlowSculptor.Domain.NetworkingProfileAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.NetworkingProfileAggregate;

/// <summary>
/// Project-level aggregate that defines the networking strategy for privatization.
/// Owned 1:0..1 by an InfrastructureConfig. Controls how Private Endpoints,
/// VNet integration, and DNS resolution are orchestrated.
/// </summary>
public sealed class NetworkingProfile : AggregateRoot<NetworkingProfileId>
{
    /// <summary>Gets the parent InfrastructureConfig identifier (required FK).</summary>
    public InfrastructureConfigId InfraConfigId { get; private set; } = null!;

    /// <summary>Gets the networking complexity mode.</summary>
    public NetworkingMode Mode { get; private set; } = null!;

    /// <summary>Gets the default VNet reference for all environments.</summary>
    public VnetReference VnetReference { get; private set; } = null!;

    /// <summary>Gets the default DNS configuration.</summary>
    public DnsConfig DnsConfig { get; private set; } = null!;

    // ─── Per-Environment Overrides ──────────────────────────────────────────────

    private readonly List<NetworkingProfileEnvironmentOverride> _environmentOverrides = [];

    /// <summary>Gets the per-environment configuration overrides.</summary>
    public IReadOnlyCollection<NetworkingProfileEnvironmentOverride> EnvironmentOverrides
        => _environmentOverrides.AsReadOnly();

    // ─── Constructor ───────────────────────────────────────────────────────────────

    private NetworkingProfile() { }

    private NetworkingProfile(
        NetworkingProfileId id,
        InfrastructureConfigId infraConfigId,
        NetworkingMode mode,
        VnetReference vnetReference,
        DnsConfig dnsConfig) : base(id)
    {
        InfraConfigId = infraConfigId;
        Mode = mode;
        VnetReference = vnetReference;
        DnsConfig = dnsConfig;
    }

    // ─── Factory ───────────────────────────────────────────────────────────────────

    /// <summary>Creates a new networking profile for an infrastructure configuration.</summary>
    public static NetworkingProfile Create(
        InfrastructureConfigId infraConfigId,
        NetworkingMode mode,
        VnetReference vnetReference,
        DnsConfig dnsConfig)
    {
        return new NetworkingProfile(
            NetworkingProfileId.CreateUnique(),
            infraConfigId,
            mode,
            vnetReference,
            dnsConfig);
    }

    // ─── Commands ──────────────────────────────────────────────────────────────────

    /// <summary>Changes the networking mode.</summary>
    public void ChangeMode(NetworkingMode newMode)
    {
        Mode = newMode;
    }

    /// <summary>Updates the default VNet reference.</summary>
    public void UpdateVnetReference(VnetReference vnetReference)
    {
        VnetReference = vnetReference;
    }

    /// <summary>Updates the default DNS configuration.</summary>
    public void UpdateDnsConfig(DnsConfig dnsConfig)
    {
        DnsConfig = dnsConfig;
    }

    /// <summary>Sets or updates an environment-specific override.</summary>
    public ErrorOr<NetworkingProfileEnvironmentOverride> SetEnvironmentOverride(
        string environmentName,
        VnetReference? vnetReferenceOverride = null,
        DnsConfig? dnsConfigOverride = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(environmentName);

        var existing = _environmentOverrides.FirstOrDefault(
            o => o.EnvironmentName.Equals(environmentName, StringComparison.OrdinalIgnoreCase));

        if (existing is not null)
        {
            existing.UpdateVnetReference(vnetReferenceOverride);
            existing.UpdateDnsConfig(dnsConfigOverride);
            return existing;
        }

        var newOverride = NetworkingProfileEnvironmentOverride.Create(
            Id, environmentName, vnetReferenceOverride, dnsConfigOverride);
        _environmentOverrides.Add(newOverride);
        return newOverride;
    }

    /// <summary>Removes an environment-specific override.</summary>
    public ErrorOr<Deleted> RemoveEnvironmentOverride(string environmentName)
    {
        var existing = _environmentOverrides.FirstOrDefault(
            o => o.EnvironmentName.Equals(environmentName, StringComparison.OrdinalIgnoreCase));

        if (existing is null)
            return Error.NotFound(
                "NetworkingProfile.EnvironmentOverrideNotFound",
                $"No override found for environment '{environmentName}'.");

        _environmentOverrides.Remove(existing);
        return Result.Deleted;
    }
}
