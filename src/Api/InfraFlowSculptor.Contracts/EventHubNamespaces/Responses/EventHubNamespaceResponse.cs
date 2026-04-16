using InfraFlowSculptor.Contracts.EventHubNamespaces.Requests;

namespace InfraFlowSculptor.Contracts.EventHubNamespaces.Responses;

/// <summary>Represents an Azure Event Hub Namespace resource.</summary>
public record EventHubNamespaceResponse(
    string Id,
    string ResourceGroupId,
    string Name,
    string Location,
    IReadOnlyCollection<EventHubNamespaceEnvironmentConfigResponse> EnvironmentSettings,
    IReadOnlyCollection<EventHubResponse> EventHubs,
    IReadOnlyCollection<EventHubConsumerGroupResponse> ConsumerGroups
);

/// <summary>Response DTO for an Event Hub.</summary>
public record EventHubResponse(string Id, string Name);

/// <summary>Response DTO for an Event Hub Consumer Group.</summary>
public record EventHubConsumerGroupResponse(string Id, string EventHubName, string ConsumerGroupName);
