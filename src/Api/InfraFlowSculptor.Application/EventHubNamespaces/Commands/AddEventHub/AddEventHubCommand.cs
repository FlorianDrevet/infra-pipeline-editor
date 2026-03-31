using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.EventHubNamespaces.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.EventHubNamespaces.Commands.AddEventHub;

/// <summary>Command to add an event hub to an Event Hub Namespace.</summary>
public record AddEventHubCommand(
    AzureResourceId EventHubNamespaceId,
    string Name
) : ICommand<EventHubNamespaceResult>;
