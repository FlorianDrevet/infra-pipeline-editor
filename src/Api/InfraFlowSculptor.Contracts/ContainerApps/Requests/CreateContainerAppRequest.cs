using System.ComponentModel.DataAnnotations;
using InfraFlowSculptor.Contracts.ValidationAttributes;

namespace InfraFlowSculptor.Contracts.ContainerApps.Requests;

/// <summary>Request body for creating a new Container App resource inside a Resource Group.</summary>
public class CreateContainerAppRequest : ContainerAppRequestBase
{
    /// <summary>Unique identifier of the Resource Group that will own this Container App.</summary>
    [Required, GuidValidation]
    public required Guid ResourceGroupId { get; init; }
}
