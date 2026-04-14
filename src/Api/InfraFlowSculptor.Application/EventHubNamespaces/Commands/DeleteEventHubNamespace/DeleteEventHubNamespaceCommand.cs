using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.EventHubNamespaces.Commands.DeleteEventHubNamespace;

/// <summary>Command to permanently delete an Event Hub Namespace resource.</summary>
public record DeleteEventHubNamespaceCommand(
    AzureResourceId Id
) : ICommand<Deleted>;
