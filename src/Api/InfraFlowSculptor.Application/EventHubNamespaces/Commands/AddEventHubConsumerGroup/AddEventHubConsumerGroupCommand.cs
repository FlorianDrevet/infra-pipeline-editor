using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.EventHubNamespaces.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.EventHubNamespaces.Commands.AddEventHubConsumerGroup;

/// <summary>Command to add a consumer group to an Event Hub Namespace.</summary>
public record AddEventHubConsumerGroupCommand(
    AzureResourceId EventHubNamespaceId,
    string EventHubName,
    string ConsumerGroupName
) : ICommand<EventHubNamespaceResult>;
