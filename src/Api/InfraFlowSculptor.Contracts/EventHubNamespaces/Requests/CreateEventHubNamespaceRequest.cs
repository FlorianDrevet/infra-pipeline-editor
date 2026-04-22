using System.ComponentModel.DataAnnotations;
using InfraFlowSculptor.Contracts.ValidationAttributes;

namespace InfraFlowSculptor.Contracts.EventHubNamespaces.Requests;

/// <summary>Request body for creating a new Event Hub Namespace resource inside a Resource Group.</summary>
public class CreateEventHubNamespaceRequest : EventHubNamespaceRequestBase
{
    /// <summary>Unique identifier of the Resource Group that will own this Event Hub Namespace.</summary>
    [Required, GuidValidation]
    public required Guid ResourceGroupId { get; init; }

    /// <summary>Whether this resource already exists in Azure and is not managed by this project.</summary>

    public bool IsExisting { get; init; } = false;

}
