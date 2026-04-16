using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Application.ServiceBusNamespaces.Common;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.ServiceBusNamespaces.Commands.CreateServiceBusNamespace;

/// <summary>Command to create a new Service Bus Namespace resource inside a Resource Group.</summary>
/// <param name="ResourceGroupId">The parent resource group identifier.</param>
/// <param name="Name">The display name.</param>
/// <param name="Location">The Azure region.</param>
/// <param name="EnvironmentSettings">Optional per-environment configuration overrides.</param>
public record CreateServiceBusNamespaceCommand(
    ResourceGroupId ResourceGroupId,
    Name Name,
    Location Location,
    IReadOnlyCollection<ServiceBusNamespaceEnvironmentConfigData>? EnvironmentSettings = null
) : ICommand<ServiceBusNamespaceResult>;
