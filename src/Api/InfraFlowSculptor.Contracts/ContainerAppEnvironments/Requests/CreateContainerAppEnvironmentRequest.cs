using System.ComponentModel.DataAnnotations;
using InfraFlowSculptor.Contracts.ValidationAttributes;

namespace InfraFlowSculptor.Contracts.ContainerAppEnvironments.Requests;

/// <summary>Request body for creating a new Container App Environment resource inside a Resource Group.</summary>
public class CreateContainerAppEnvironmentRequest : ContainerAppEnvironmentRequestBase
{
    /// <summary>Unique identifier of the Resource Group that will own this Container App Environment.</summary>
    [Required, GuidValidation]
    public required Guid ResourceGroupId { get; init; }
}
