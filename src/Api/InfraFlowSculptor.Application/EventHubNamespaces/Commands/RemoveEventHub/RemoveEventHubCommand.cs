using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.EventHubNamespaces.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.EventHubNamespaces.Commands.RemoveEventHub;

/// <summary>Command to remove an event hub from an Event Hub Namespace.</summary>
public record RemoveEventHubCommand(
    AzureResourceId EventHubNamespaceId,
    Guid EventHubId
) : ICommand<EventHubNamespaceResult>;
