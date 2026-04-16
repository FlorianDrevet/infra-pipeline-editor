using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.EventHubNamespaces.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;

namespace InfraFlowSculptor.Application.EventHubNamespaces.Commands.UpdateEventHubNamespace;

/// <summary>Command to update an existing Event Hub Namespace resource.</summary>
public record UpdateEventHubNamespaceCommand(
    AzureResourceId Id,
    Name Name,
    Location Location,
    IReadOnlyCollection<EventHubNamespaceEnvironmentConfigData>? EnvironmentSettings = null
) : ICommand<EventHubNamespaceResult>;
