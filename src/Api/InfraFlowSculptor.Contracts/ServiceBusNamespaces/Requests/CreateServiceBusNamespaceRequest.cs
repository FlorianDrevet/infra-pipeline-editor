using System.ComponentModel.DataAnnotations;
using InfraFlowSculptor.Contracts.ValidationAttributes;

namespace InfraFlowSculptor.Contracts.ServiceBusNamespaces.Requests;

/// <summary>Request body for creating a new Service Bus Namespace resource inside a Resource Group.</summary>
public class CreateServiceBusNamespaceRequest : ServiceBusNamespaceRequestBase
{
    /// <summary>Unique identifier of the Resource Group that will own this Service Bus Namespace.</summary>
    [Required, GuidValidation]
    public required Guid ResourceGroupId { get; init; }

    /// <summary>Whether this resource already exists in Azure and is not managed by this project.</summary>

    public bool IsExisting { get; init; } = false;

}
