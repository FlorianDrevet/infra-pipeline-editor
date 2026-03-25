using ErrorOr;
using InfraFlowSculptor.Application.ServiceBusNamespaces.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.ServiceBusNamespaces.Commands.UpdateServiceBusNamespace;

/// <summary>Command to update an existing Service Bus Namespace resource.</summary>
/// <param name="Id">The Service Bus Namespace identifier.</param>
/// <param name="Name">The new display name.</param>
/// <param name="Location">The new Azure region.</param>
/// <param name="EnvironmentSettings">Optional per-environment configuration overrides.</param>
public record UpdateServiceBusNamespaceCommand(
    AzureResourceId Id,
    Name Name,
    Location Location,
    IReadOnlyList<ServiceBusNamespaceEnvironmentConfigData>? EnvironmentSettings = null
) : IRequest<ErrorOr<ServiceBusNamespaceResult>>;
