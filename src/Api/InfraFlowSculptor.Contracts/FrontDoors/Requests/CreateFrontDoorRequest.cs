using System.ComponentModel.DataAnnotations;
using InfraFlowSculptor.Contracts.ValidationAttributes;

namespace InfraFlowSculptor.Contracts.FrontDoors.Requests;

/// <summary>Request body for creating a new Front Door resource.</summary>
public class CreateFrontDoorRequest : FrontDoorRequestBase
{
    /// <summary>Unique identifier of the Resource Group that will own this Front Door.</summary>
    [Required, GuidValidation]
    public required Guid ResourceGroupId { get; init; }

    /// <summary>Whether this resource already exists in Azure and is not managed by this project.</summary>
    public bool IsExisting { get; init; } = false;
}
