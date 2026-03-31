using InfraFlowSculptor.Contracts.EventHubNamespaces.Requests;

namespace InfraFlowSculptor.Contracts.EventHubNamespaces.Responses;

/// <summary>Represents an Azure Event Hub Namespace resource.</summary>
public record EventHubNamespaceResponse(
    Guid Id,
    Guid ResourceGroupId,
    string Name,
    string Location,
    IReadOnlyList<EventHubNamespaceEnvironmentConfigResponse> EnvironmentSettings,
    IReadOnlyList<EventHubResponse> EventHubs,
    IReadOnlyList<EventHubConsumerGroupResponse> ConsumerGroups
);

/// <summary>Response DTO for an Event Hub.</summary>
public record EventHubResponse(Guid Id, string Name);

/// <summary>Response DTO for an Event Hub Consumer Group.</summary>
public record EventHubConsumerGroupResponse(Guid Id, string EventHubName, string ConsumerGroupName);
