using InfraFlowSculptor.Domain.Common.BaseModels;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.Entities;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects.ResourceParameterUsage;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using InfraFlowSculptor.Domain.ServiceBusNamespaceAggregate.Entities;
using InfraFlowSculptor.Domain.ServiceBusNamespaceAggregate.ValueObjects;
using ErrorOr;

namespace InfraFlowSculptor.Domain.ServiceBusNamespaceAggregate;

/// <summary>
/// Represents an Azure Service Bus Namespace resource aggregate root.
/// Manages queues and topic subscriptions as sub-resources.
/// </summary>
public class ServiceBusNamespace : AzureResource
{
    private readonly List<ServiceBusNamespaceEnvironmentSettings> _environmentSettings = [];
    private readonly List<ServiceBusQueue> _queues = [];
    private readonly List<ServiceBusTopicSubscription> _topicSubscriptions = [];

    /// <summary>Gets the typed per-environment configuration overrides for this Service Bus Namespace.</summary>
    public IReadOnlyCollection<ServiceBusNamespaceEnvironmentSettings> EnvironmentSettings => _environmentSettings.AsReadOnly();

    /// <summary>Gets the queues defined in this Service Bus Namespace.</summary>
    public IReadOnlyCollection<ServiceBusQueue> Queues => _queues.AsReadOnly();

    /// <summary>Gets the topic subscriptions defined in this Service Bus Namespace.</summary>
    public IReadOnlyCollection<ServiceBusTopicSubscription> TopicSubscriptions => _topicSubscriptions.AsReadOnly();

    /// <inheritdoc />
    protected override IReadOnlyCollection<ParameterUsage> AllowedParameterUsages =>
        Array.Empty<ParameterUsage>();

    private ServiceBusNamespace()
    {
    }

    /// <summary>
    /// Updates the mutable properties of this Service Bus Namespace resource.
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
        string? minimumTlsVersion)
    {
        var existing = _environmentSettings.FirstOrDefault(
            es => es.EnvironmentName == environmentName);

        if (existing is not null)
        {
            existing.Update(sku, capacity, zoneRedundant, disableLocalAuth, minimumTlsVersion);
        }
        else
        {
            _environmentSettings.Add(
                ServiceBusNamespaceEnvironmentSettings.Create(
                    Id, environmentName, sku, capacity, zoneRedundant, disableLocalAuth, minimumTlsVersion));
        }
    }

    /// <summary>
    /// Sets all per-environment settings at once, replacing any existing entries.
    /// </summary>
    public void SetAllEnvironmentSettings(
        IReadOnlyList<(string EnvironmentName, string? Sku, int? Capacity, bool? ZoneRedundant, bool? DisableLocalAuth, string? MinimumTlsVersion)> settings)
    {
        _environmentSettings.Clear();
        foreach (var (envName, sku, capacity, zoneRedundant, disableLocalAuth, minimumTlsVersion) in settings)
        {
            _environmentSettings.Add(
                ServiceBusNamespaceEnvironmentSettings.Create(
                    Id, envName, sku, capacity, zoneRedundant, disableLocalAuth, minimumTlsVersion));
        }
    }

    /// <summary>
    /// Adds a queue to this Service Bus Namespace.
    /// </summary>
    /// <param name="name">The queue name.</param>
    /// <returns>The created queue, or an error if a duplicate name exists.</returns>
    public ErrorOr<ServiceBusQueue> AddQueue(string name)
    {
        if (_queues.Any(q => string.Equals(q.Name, name, StringComparison.OrdinalIgnoreCase)))
            return Domain.Common.Errors.Errors.ServiceBusNamespace.DuplicateQueueName(name);

        var queue = ServiceBusQueue.Create(Id, name);
        _queues.Add(queue);
        return queue;
    }

    /// <summary>
    /// Removes a queue from this Service Bus Namespace.
    /// </summary>
    /// <param name="queueId">The queue identifier to remove.</param>
    /// <returns>Success or a not-found error.</returns>
    public ErrorOr<Deleted> RemoveQueue(ServiceBusQueueId queueId)
    {
        var queue = _queues.FirstOrDefault(q => q.Id == queueId);
        if (queue is null)
            return Domain.Common.Errors.Errors.ServiceBusNamespace.QueueNotFound(queueId);

        _queues.Remove(queue);
        return Result.Deleted;
    }

    /// <summary>
    /// Adds a topic subscription to this Service Bus Namespace.
    /// </summary>
    /// <param name="topicName">The topic name.</param>
    /// <param name="subscriptionName">The subscription name.</param>
    /// <returns>The created subscription, or an error if a duplicate exists.</returns>
    public ErrorOr<ServiceBusTopicSubscription> AddTopicSubscription(string topicName, string subscriptionName)
    {
        if (_topicSubscriptions.Any(ts =>
                string.Equals(ts.TopicName, topicName, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(ts.SubscriptionName, subscriptionName, StringComparison.OrdinalIgnoreCase)))
            return Domain.Common.Errors.Errors.ServiceBusNamespace.DuplicateTopicSubscription(topicName, subscriptionName);

        var sub = ServiceBusTopicSubscription.Create(Id, topicName, subscriptionName);
        _topicSubscriptions.Add(sub);
        return sub;
    }

    /// <summary>
    /// Removes a topic subscription from this Service Bus Namespace.
    /// </summary>
    /// <param name="subscriptionId">The subscription identifier to remove.</param>
    /// <returns>Success or a not-found error.</returns>
    public ErrorOr<Deleted> RemoveTopicSubscription(ServiceBusTopicSubscriptionId subscriptionId)
    {
        var sub = _topicSubscriptions.FirstOrDefault(ts => ts.Id == subscriptionId);
        if (sub is null)
            return Domain.Common.Errors.Errors.ServiceBusNamespace.TopicSubscriptionNotFound(subscriptionId);

        _topicSubscriptions.Remove(sub);
        return Result.Deleted;
    }

    /// <summary>
    /// Creates a new <see cref="ServiceBusNamespace"/> instance with a generated identifier.
    /// </summary>
    /// <param name="resourceGroupId">The parent resource group identifier.</param>
    /// <param name="name">The display name.</param>
    /// <param name="location">The Azure region.</param>
    /// <param name="environmentSettings">Optional per-environment configuration overrides.</param>
    /// <returns>A new <see cref="ServiceBusNamespace"/> aggregate root.</returns>
    public static ServiceBusNamespace Create(
        ResourceGroupId resourceGroupId,
        Name name,
        Location location,
        IReadOnlyList<(string EnvironmentName, string? Sku, int? Capacity, bool? ZoneRedundant, bool? DisableLocalAuth, string? MinimumTlsVersion)>? environmentSettings = null)
    {
        var sb = new ServiceBusNamespace
        {
            Id = AzureResourceId.CreateUnique(),
            ResourceGroupId = resourceGroupId,
            Name = name,
            Location = location
        };

        if (environmentSettings is not null)
            sb.SetAllEnvironmentSettings(environmentSettings);

        return sb;
    }
}
