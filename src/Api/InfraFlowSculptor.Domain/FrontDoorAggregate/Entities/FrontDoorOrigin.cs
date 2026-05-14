using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.FrontDoorAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.FrontDoorAggregate.Entities;

/// <summary>Represents an origin backend in a Front Door profile.</summary>
public sealed class FrontDoorOrigin : Entity<FrontDoorOriginId>
{
    /// <summary>Gets the parent Front Door identifier.</summary>
    public AzureResourceId FrontDoorId { get; private set; } = null!;

    /// <summary>Gets the target resource identifier (WebApp, ContainerApp, StorageAccount).</summary>
    public AzureResourceId TargetResourceId { get; private set; } = null!;

    /// <summary>Gets the host name override (null = auto-derived from resource).</summary>
    public string? HostName { get; private set; }

    /// <summary>Gets whether private link is used to connect to this origin.</summary>
    public bool PrivateLinkEnabled { get; private set; }

    /// <summary>Gets the traffic weight for load balancing.</summary>
    public int Weight { get; private set; }

    /// <summary>Gets the priority for failover ordering.</summary>
    public int Priority { get; private set; }

    private FrontDoorOrigin() { }

    /// <summary>Updates origin properties.</summary>
    public void Update(string? hostName, bool privateLinkEnabled, int weight, int priority)
    {
        HostName = hostName;
        PrivateLinkEnabled = privateLinkEnabled;
        Weight = weight;
        Priority = priority;
    }

    /// <summary>Creates a new origin.</summary>
    internal static FrontDoorOrigin Create(AzureResourceId frontDoorId, AzureResourceId targetResourceId, string? hostName, bool privateLinkEnabled, int weight, int priority)
    {
        return new FrontDoorOrigin
        {
            Id = FrontDoorOriginId.CreateUnique(),
            FrontDoorId = frontDoorId,
            TargetResourceId = targetResourceId,
            HostName = hostName,
            PrivateLinkEnabled = privateLinkEnabled,
            Weight = weight,
            Priority = priority
        };
    }
}
