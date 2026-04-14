using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.ServiceBusNamespaceAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.ServiceBusNamespaceAggregate.Entities;

/// <summary>
/// Represents a topic subscription within an Azure Service Bus Namespace.
/// </summary>
public sealed class ServiceBusTopicSubscription : Entity<ServiceBusTopicSubscriptionId>
{
    /// <summary>Gets the parent Service Bus Namespace identifier.</summary>
    public AzureResourceId ServiceBusNamespaceId { get; private set; } = null!;

    /// <summary>Gets the topic name.</summary>
    public string TopicName { get; private set; } = string.Empty;

    /// <summary>Gets the subscription name within the topic.</summary>
    public string SubscriptionName { get; private set; } = string.Empty;

    private ServiceBusTopicSubscription() { }

    /// <summary>
    /// Creates a new <see cref="ServiceBusTopicSubscription"/> for the specified namespace.
    /// </summary>
    /// <param name="serviceBusNamespaceId">The parent namespace identifier.</param>
    /// <param name="topicName">The topic name.</param>
    /// <param name="subscriptionName">The subscription name.</param>
    /// <returns>A new <see cref="ServiceBusTopicSubscription"/> entity.</returns>
    public static ServiceBusTopicSubscription Create(
        AzureResourceId serviceBusNamespaceId,
        string topicName,
        string subscriptionName)
        => new()
        {
            Id = ServiceBusTopicSubscriptionId.CreateUnique(),
            ServiceBusNamespaceId = serviceBusNamespaceId,
            TopicName = topicName,
            SubscriptionName = subscriptionName
        };
}
