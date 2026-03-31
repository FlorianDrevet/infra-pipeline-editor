using ErrorOr;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.EventHubNamespaceAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.Common.Errors;

public static partial class Errors
{
    /// <summary>Domain errors for the <see cref="EventHubNamespaceAggregate.EventHubNamespace"/> aggregate.</summary>
    public static class EventHubNamespace
    {
        /// <summary>Returns a not-found error for the given identifier.</summary>
        public static Error NotFoundError(AzureResourceId id) => Error.NotFound(
            code: "EventHubNamespace.NotFound",
            description: $"An Event Hub Namespace with the given id {id} does not exist.",
            metadata: new Dictionary<string, object> { { "Id", id.ToString() } }
        );

        /// <summary>Returns an error when an event hub with the same name already exists.</summary>
        public static Error DuplicateEventHubName(string name) => Error.Conflict(
            code: "EventHubNamespace.DuplicateEventHubName",
            description: $"An event hub with the name '{name}' already exists in this Event Hub Namespace.");

        /// <summary>Returns a not-found error for an event hub.</summary>
        public static Error EventHubNotFound(EventHubId id) => Error.NotFound(
            code: "EventHubNamespace.EventHubNotFound",
            description: $"An event hub with the given id {id} does not exist in this Event Hub Namespace.");

        /// <summary>Returns an error when a consumer group with the same names already exists.</summary>
        public static Error DuplicateConsumerGroup(string eventHubName, string consumerGroupName) => Error.Conflict(
            code: "EventHubNamespace.DuplicateConsumerGroup",
            description: $"A consumer group '{consumerGroupName}' on event hub '{eventHubName}' already exists in this Event Hub Namespace.");

        /// <summary>Returns a not-found error for a consumer group.</summary>
        public static Error ConsumerGroupNotFound(EventHubConsumerGroupId id) => Error.NotFound(
            code: "EventHubNamespace.ConsumerGroupNotFound",
            description: $"A consumer group with the given id {id} does not exist in this Event Hub Namespace.");
    }
}
