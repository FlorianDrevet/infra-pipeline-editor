using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.EventHubNamespaces.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.EventHubNamespaces.Commands.RemoveEventHubConsumerGroup;

/// <summary>Command to remove a consumer group from an Event Hub Namespace.</summary>
public record RemoveEventHubConsumerGroupCommand(
    AzureResourceId EventHubNamespaceId,
    Guid ConsumerGroupId
) : ICommand<EventHubNamespaceResult>;
