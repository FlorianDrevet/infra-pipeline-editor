using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.NetworkingProfileAggregate.ValueObjects;

/// <summary>
/// Encapsulates the VNet reference configuration for a networking profile.
/// Describes how the project resolves its VNet (new, existing, or hub-spoke).
/// </summary>
public sealed class VnetReference : ValueObject
{
    /// <summary>Gets the VNet sourcing strategy.</summary>
    public VnetSource Source { get; private set; }

    /// <summary>Gets the Azure resource ID of an existing VNet (when <see cref="Source"/> is <see cref="VnetSource.SourceType.UseExisting"/> or <see cref="VnetSource.SourceType.UseHubSpoke"/>).</summary>
    public string? ExistingVnetResourceId { get; private set; }

    /// <summary>Gets the address space for a newly created VNet (when <see cref="Source"/> is <see cref="VnetSource.SourceType.CreateNew"/>).</summary>
    public CidrBlock? CreateNewAddressSpace { get; private set; }

    /// <summary>Gets the subnet name dedicated to private endpoints.</summary>
    public string PrivateEndpointsSubnetName { get; private set; }

    /// <summary>Gets the CIDR block for the private endpoints subnet (when creating new).</summary>
    public CidrBlock? PrivateEndpointsSubnetAddressPrefix { get; private set; }

    private const string DefaultSubnetName = "snet-pe";

    private VnetReference()
    {
        Source = new VnetSource(VnetSource.SourceType.CreateNew);
        PrivateEndpointsSubnetName = DefaultSubnetName;
    }

    /// <summary>Creates a VNet reference for a newly created VNet.</summary>
    public static VnetReference CreateNew(CidrBlock addressSpace, CidrBlock subnetAddressPrefix, string? subnetName = null)
    {
        return new VnetReference
        {
            Source = new VnetSource(VnetSource.SourceType.CreateNew),
            CreateNewAddressSpace = addressSpace,
            PrivateEndpointsSubnetAddressPrefix = subnetAddressPrefix,
            PrivateEndpointsSubnetName = subnetName ?? DefaultSubnetName
        };
    }

    /// <summary>Creates a VNet reference for an existing VNet.</summary>
    public static VnetReference UseExisting(string existingVnetResourceId, string subnetName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(existingVnetResourceId);
        ArgumentException.ThrowIfNullOrWhiteSpace(subnetName);

        return new VnetReference
        {
            Source = new VnetSource(VnetSource.SourceType.UseExisting),
            ExistingVnetResourceId = existingVnetResourceId,
            PrivateEndpointsSubnetName = subnetName
        };
    }

    /// <summary>Creates a VNet reference for a hub-and-spoke topology.</summary>
    public static VnetReference UseHubSpoke(string hubVnetResourceId, string subnetName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(hubVnetResourceId);
        ArgumentException.ThrowIfNullOrWhiteSpace(subnetName);

        return new VnetReference
        {
            Source = new VnetSource(VnetSource.SourceType.UseHubSpoke),
            ExistingVnetResourceId = hubVnetResourceId,
            PrivateEndpointsSubnetName = subnetName
        };
    }

    /// <inheritdoc />
    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Source;
        yield return ExistingVnetResourceId ?? string.Empty;
        yield return CreateNewAddressSpace?.Value ?? string.Empty;
        yield return PrivateEndpointsSubnetName;
        yield return PrivateEndpointsSubnetAddressPrefix?.Value ?? string.Empty;
    }
}
