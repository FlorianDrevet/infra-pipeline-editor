using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.EventHubNamespaces.Common;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.EventHubNamespaces.Commands.CreateEventHubNamespace;

/// <summary>Command to create a new Event Hub Namespace resource inside a Resource Group.</summary>
public record CreateEventHubNamespaceCommand(
    ResourceGroupId ResourceGroupId,
    Name Name,
    Location Location,
    IReadOnlyList<EventHubNamespaceEnvironmentConfigData>? EnvironmentSettings = null,
    bool IsExisting = false
) : ICommand<EventHubNamespaceResult>;
