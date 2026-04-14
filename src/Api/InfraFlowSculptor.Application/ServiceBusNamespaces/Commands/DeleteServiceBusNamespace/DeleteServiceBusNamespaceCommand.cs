using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.ServiceBusNamespaces.Commands.DeleteServiceBusNamespace;

/// <summary>Command to permanently delete a Service Bus Namespace resource.</summary>
/// <param name="Id">The Service Bus Namespace identifier.</param>
public record DeleteServiceBusNamespaceCommand(
    AzureResourceId Id
) : ICommand<Deleted>;
