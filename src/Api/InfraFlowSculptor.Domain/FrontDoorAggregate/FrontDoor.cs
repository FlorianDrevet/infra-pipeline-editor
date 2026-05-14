using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.FrontDoorAggregate.Entities;
using InfraFlowSculptor.Domain.FrontDoorAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.FrontDoorAggregate;

/// <summary>
/// Represents an Azure Front Door profile (<c>Microsoft.Cdn/profiles</c>).
/// Provides global load balancing, CDN, and WAF capabilities for privatized services.
/// </summary>
public sealed class FrontDoor : AzureResource
{
    private readonly List<FrontDoorOrigin> _origins = [];
    /// <summary>Gets the origin configurations.</summary>
    public IReadOnlyCollection<FrontDoorOrigin> Origins => _origins.AsReadOnly();

    private readonly List<FrontDoorEnvironmentSettings> _environmentSettings = [];
    /// <summary>Gets the per-environment configuration.</summary>
    public IReadOnlyCollection<FrontDoorEnvironmentSettings> EnvironmentSettings => _environmentSettings.AsReadOnly();

    /// <summary>Whether WAF policy is enabled on this Front Door profile.</summary>
    public bool WafPolicyEnabled { get; private set; }

    private FrontDoor() { }

    /// <summary>Updates resource-level properties.</summary>
    public void Update(Name name, Location location, bool wafPolicyEnabled)
    {
        SetNameAndLocation(name, location);
        if (IsExisting) return;
        WafPolicyEnabled = wafPolicyEnabled;
    }

    /// <summary>Adds an origin to this Front Door profile.</summary>
    public FrontDoorOrigin AddOrigin(AzureResourceId targetResourceId, string? hostName, bool privateLinkEnabled, int weight, int priority)
    {
        var origin = FrontDoorOrigin.Create(Id, targetResourceId, hostName, privateLinkEnabled, weight, priority);
        _origins.Add(origin);
        return origin;
    }

    /// <summary>Updates an existing origin.</summary>
    public void UpdateOrigin(FrontDoorOriginId originId, string? hostName, bool privateLinkEnabled, int weight, int priority)
    {
        var origin = _origins.FirstOrDefault(o => o.Id == originId)
            ?? throw new InvalidOperationException($"Origin '{originId.Value}' not found.");
        origin.Update(hostName, privateLinkEnabled, weight, priority);
    }

    /// <summary>Removes an origin.</summary>
    public void RemoveOrigin(FrontDoorOriginId originId)
    {
        var origin = _origins.FirstOrDefault(o => o.Id == originId)
            ?? throw new InvalidOperationException($"Origin '{originId.Value}' not found.");
        _origins.Remove(origin);
    }

    /// <summary>Sets per-environment settings.</summary>
    public void SetEnvironmentSettings(string environmentName, FrontDoorSku sku)
    {
        if (IsExisting) return;
        var existing = _environmentSettings.FirstOrDefault(es => es.EnvironmentName == environmentName);
        if (existing is not null)
            existing.Update(sku);
        else
            _environmentSettings.Add(FrontDoorEnvironmentSettings.Create(Id, environmentName, sku));
    }

    /// <summary>Sets all per-environment settings at once.</summary>
    public void SetAllEnvironmentSettings(IReadOnlyList<(string EnvironmentName, FrontDoorSku Sku)> settings)
    {
        if (IsExisting) return;
        _environmentSettings.Clear();
        foreach (var (envName, sku) in settings)
            _environmentSettings.Add(FrontDoorEnvironmentSettings.Create(Id, envName, sku));
    }

    /// <summary>Creates a new FrontDoor.</summary>
    public static FrontDoor Create(
        ResourceGroupId resourceGroupId,
        Name name,
        Location location,
        bool wafPolicyEnabled = false,
        IReadOnlyList<(string EnvironmentName, FrontDoorSku Sku)>? environmentSettings = null,
        bool isExisting = false)
    {
        var fd = new FrontDoor
        {
            Id = AzureResourceId.CreateUnique(),
            ResourceGroupId = resourceGroupId,
            Name = name,
            Location = location,
            IsExisting = isExisting,
            WafPolicyEnabled = wafPolicyEnabled
        };
        if (!isExisting && environmentSettings is not null)
            fd.SetAllEnvironmentSettings(environmentSettings);
        return fd;
    }
}
