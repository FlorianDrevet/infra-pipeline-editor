using System.ComponentModel.DataAnnotations;
using InfraFlowSculptor.Contracts.ValidationAttributes;

namespace InfraFlowSculptor.Contracts.ContainerRegistries.Requests;

/// <summary>Request body for creating a new Container Registry resource inside a Resource Group.</summary>
public class CreateContainerRegistryRequest : ContainerRegistryRequestBase
{
    /// <summary>Unique identifier of the Resource Group that will own this Container Registry.</summary>
    [Required, GuidValidation]
    public required Guid ResourceGroupId { get; init; }
}
