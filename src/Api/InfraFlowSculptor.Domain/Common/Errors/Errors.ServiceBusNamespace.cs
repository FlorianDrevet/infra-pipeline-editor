using ErrorOr;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.ServiceBusNamespaceAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.Common.Errors;

public static partial class Errors
{
    /// <summary>Domain errors for the <see cref="ServiceBusNamespaceAggregate.ServiceBusNamespace"/> aggregate.</summary>
    public static class ServiceBusNamespace
    {
        /// <summary>Returns a not-found error for the given identifier.</summary>
        public static Error NotFoundError(AzureResourceId id) => Error.NotFound(
            code: "ServiceBusNamespace.NotFound",
            description: $"A Service Bus Namespace with the given id {id} does not exist.",
            metadata: new Dictionary<string, object> { { "Id", id.ToString() } }
        );

        /// <summary>Returns an error when a queue with the same name already exists.</summary>
        public static Error DuplicateQueueName(string name) => Error.Conflict(
            code: "ServiceBusNamespace.DuplicateQueueName",
            description: $"A queue with the name '{name}' already exists in this Service Bus Namespace.");

        /// <summary>Returns a not-found error for a queue.</summary>
        public static Error QueueNotFound(ServiceBusQueueId id) => Error.NotFound(
            code: "ServiceBusNamespace.QueueNotFound",
            description: $"A queue with the given id {id} does not exist in this Service Bus Namespace.");

        /// <summary>Returns an error when a topic subscription with the same names already exists.</summary>
        public static Error DuplicateTopicSubscription(string topicName, string subscriptionName) => Error.Conflict(
            code: "ServiceBusNamespace.DuplicateTopicSubscription",
            description: $"A subscription '{subscriptionName}' on topic '{topicName}' already exists in this Service Bus Namespace.");

        /// <summary>Returns a not-found error for a topic subscription.</summary>
        public static Error TopicSubscriptionNotFound(ServiceBusTopicSubscriptionId id) => Error.NotFound(
            code: "ServiceBusNamespace.TopicSubscriptionNotFound",
            description: $"A topic subscription with the given id {id} does not exist in this Service Bus Namespace.");
    }
}
