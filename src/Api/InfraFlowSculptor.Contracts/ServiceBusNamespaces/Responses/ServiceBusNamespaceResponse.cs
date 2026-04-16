namespace InfraFlowSculptor.Contracts.ServiceBusNamespaces.Responses;

/// <summary>Represents an Azure Service Bus Namespace resource.</summary>
/// <param name="Id">Unique identifier of the Service Bus Namespace.</param>
/// <param name="ResourceGroupId">Identifier of the parent Resource Group.</param>
/// <param name="Name">Display name of the Service Bus Namespace.</param>
/// <param name="Location">Azure region where the Service Bus Namespace is deployed.</param>
/// <param name="EnvironmentSettings">Per-environment typed configuration overrides.</param>
/// <param name="Queues">Queues defined in this Service Bus Namespace.</param>
/// <param name="TopicSubscriptions">Topic subscriptions defined in this Service Bus Namespace.</param>
public record ServiceBusNamespaceResponse(
    string Id,
    string ResourceGroupId,
    string Name,
    string Location,
    IReadOnlyCollection<ServiceBusNamespaceEnvironmentConfigResponse> EnvironmentSettings,
    IReadOnlyCollection<ServiceBusQueueResponse> Queues,
    IReadOnlyCollection<ServiceBusTopicSubscriptionResponse> TopicSubscriptions
);

/// <summary>Response DTO for a Service Bus Queue.</summary>
public record ServiceBusQueueResponse(string Id, string Name);

/// <summary>Response DTO for a Service Bus Topic Subscription.</summary>
public record ServiceBusTopicSubscriptionResponse(string Id, string TopicName, string SubscriptionName);

/// <summary>Response DTO for a typed per-environment Service Bus Namespace configuration.</summary>
public record ServiceBusNamespaceEnvironmentConfigResponse(
    string EnvironmentName,
    string? Sku,
    int? Capacity,
    bool? ZoneRedundant,
    bool? DisableLocalAuth,
    string? MinimumTlsVersion);
