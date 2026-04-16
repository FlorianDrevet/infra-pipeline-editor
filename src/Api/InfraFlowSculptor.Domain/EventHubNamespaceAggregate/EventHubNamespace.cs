using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects.ResourceParameterUsage;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using InfraFlowSculptor.Domain.EventHubNamespaceAggregate.Entities;
using InfraFlowSculptor.Domain.EventHubNamespaceAggregate.ValueObjects;
using ErrorOr;

namespace InfraFlowSculptor.Domain.EventHubNamespaceAggregate;

/// <summary>
/// Represents an Azure Event Hub Namespace resource aggregate root.
/// Manages event hubs and consumer groups as sub-resources.
/// </summary>
public sealed class EventHubNamespace : AzureResource
{
    private readonly List<EventHubNamespaceEnvironmentSettings> _environmentSettings = [];
    private readonly List<EventHub> _eventHubs = [];
    private readonly List<EventHubConsumerGroup> _consumerGroups = [];

    /// <summary>Gets the typed per-environment configuration overrides for this Event Hub Namespace.</summary>
    public IReadOnlyCollection<EventHubNamespaceEnvironmentSettings> EnvironmentSettings => _environmentSettings.AsReadOnly();

    /// <summary>Gets the event hubs defined in this Event Hub Namespace.</summary>
    public IReadOnlyCollection<EventHub> EventHubs => _eventHubs.AsReadOnly();

    /// <summary>Gets the consumer groups defined in this Event Hub Namespace.</summary>
    public IReadOnlyCollection<EventHubConsumerGroup> ConsumerGroups => _consumerGroups.AsReadOnly();

    /// <inheritdoc />
    protected override IReadOnlyCollection<ParameterUsage> AllowedParameterUsages =>
        Array.Empty<ParameterUsage>();

    private EventHubNamespace()
    {
    }

    /// <summary>
    /// Updates the mutable properties of this Event Hub Namespace resource.
    /// </summary>
    /// <param name="name">The new display name.</param>
    /// <param name="location">The new Azure region.</param>
    public void Update(Name name, Location location)
    {
        Name = name;
        Location = location;
    }

    /// <summary>
    /// Sets the per-environment settings for the given environment.
    /// Replaces existing settings if one already exists for this environment.
    /// </summary>
    public void SetEnvironmentSettings(
        string environmentName,
        string? sku,
        int? capacity,
        bool? zoneRedundant,
        bool? disableLocalAuth,
        string? minimumTlsVersion,
        bool? autoInflateEnabled,
        int? maxThroughputUnits)
    {
        var existing = _environmentSettings.FirstOrDefault(
            es => es.EnvironmentName == environmentName);

        if (existing is not null)
        {
            existing.Update(sku, capacity, zoneRedundant, disableLocalAuth, minimumTlsVersion, autoInflateEnabled, maxThroughputUnits);
        }
        else
        {
            _environmentSettings.Add(
                EventHubNamespaceEnvironmentSettings.Create(
                    Id, environmentName, sku, capacity, zoneRedundant, disableLocalAuth, minimumTlsVersion, autoInflateEnabled, maxThroughputUnits));
        }
    }

    /// <summary>
    /// Sets all per-environment settings at once, replacing any existing entries.
    /// </summary>
    public void SetAllEnvironmentSettings(
        IReadOnlyCollection<(string EnvironmentName, string? Sku, int? Capacity, bool? ZoneRedundant, bool? DisableLocalAuth, string? MinimumTlsVersion, bool? AutoInflateEnabled, int? MaxThroughputUnits)> settings)
    {
        _environmentSettings.Clear();
        foreach (var (envName, sku, capacity, zoneRedundant, disableLocalAuth, minimumTlsVersion, autoInflateEnabled, maxThroughputUnits) in settings)
        {
            _environmentSettings.Add(
                EventHubNamespaceEnvironmentSettings.Create(
                    Id, envName, sku, capacity, zoneRedundant, disableLocalAuth, minimumTlsVersion, autoInflateEnabled, maxThroughputUnits));
        }
    }

    /// <summary>
    /// Adds an event hub to this Event Hub Namespace.
    /// </summary>
    /// <param name="name">The event hub name.</param>
    /// <returns>The created event hub, or an error if a duplicate name exists.</returns>
    public ErrorOr<EventHub> AddEventHub(string name)
    {
        if (_eventHubs.Any(eh => string.Equals(eh.Name, name, StringComparison.OrdinalIgnoreCase)))
            return Domain.Common.Errors.Errors.EventHubNamespace.DuplicateEventHubName(name);

        var eventHub = EventHub.Create(Id, name);
        _eventHubs.Add(eventHub);
        return eventHub;
    }

    /// <summary>
    /// Removes an event hub from this Event Hub Namespace.
    /// </summary>
    /// <param name="eventHubId">The event hub identifier to remove.</param>
    /// <returns>Success or a not-found error.</returns>
    public ErrorOr<Deleted> RemoveEventHub(EventHubId eventHubId)
    {
        var eventHub = _eventHubs.FirstOrDefault(eh => eh.Id == eventHubId);
        if (eventHub is null)
            return Domain.Common.Errors.Errors.EventHubNamespace.EventHubNotFound(eventHubId);

        _eventHubs.Remove(eventHub);
        return Result.Deleted;
    }

    /// <summary>
    /// Adds a consumer group to this Event Hub Namespace.
    /// </summary>
    /// <param name="eventHubName">The event hub name.</param>
    /// <param name="consumerGroupName">The consumer group name.</param>
    /// <returns>The created consumer group, or an error if a duplicate exists.</returns>
    public ErrorOr<EventHubConsumerGroup> AddConsumerGroup(string eventHubName, string consumerGroupName)
    {
        if (_consumerGroups.Any(cg =>
                string.Equals(cg.EventHubName, eventHubName, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(cg.ConsumerGroupName, consumerGroupName, StringComparison.OrdinalIgnoreCase)))
            return Domain.Common.Errors.Errors.EventHubNamespace.DuplicateConsumerGroup(eventHubName, consumerGroupName);

        var cg = EventHubConsumerGroup.Create(Id, eventHubName, consumerGroupName);
        _consumerGroups.Add(cg);
        return cg;
    }

    /// <summary>
    /// Removes a consumer group from this Event Hub Namespace.
    /// </summary>
    /// <param name="consumerGroupId">The consumer group identifier to remove.</param>
    /// <returns>Success or a not-found error.</returns>
    public ErrorOr<Deleted> RemoveConsumerGroup(EventHubConsumerGroupId consumerGroupId)
    {
        var cg = _consumerGroups.FirstOrDefault(c => c.Id == consumerGroupId);
        if (cg is null)
            return Domain.Common.Errors.Errors.EventHubNamespace.ConsumerGroupNotFound(consumerGroupId);

        _consumerGroups.Remove(cg);
        return Result.Deleted;
    }

    /// <summary>
    /// Creates a new <see cref="EventHubNamespace"/> instance with a generated identifier.
    /// </summary>
    public static EventHubNamespace Create(
        ResourceGroupId resourceGroupId,
        Name name,
        Location location,
        IReadOnlyCollection<(string EnvironmentName, string? Sku, int? Capacity, bool? ZoneRedundant, bool? DisableLocalAuth, string? MinimumTlsVersion, bool? AutoInflateEnabled, int? MaxThroughputUnits)>? environmentSettings = null)
    {
        var eh = new EventHubNamespace
        {
            Id = AzureResourceId.CreateUnique(),
            ResourceGroupId = resourceGroupId,
            Name = name,
            Location = location
        };

        if (environmentSettings is not null)
            eh.SetAllEnvironmentSettings(environmentSettings);

        return eh;
    }
}
