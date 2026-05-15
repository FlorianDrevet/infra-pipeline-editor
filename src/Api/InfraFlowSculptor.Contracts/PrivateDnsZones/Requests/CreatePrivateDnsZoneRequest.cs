using System.ComponentModel.DataAnnotations;
using InfraFlowSculptor.Contracts.ValidationAttributes;

namespace InfraFlowSculptor.Contracts.PrivateDnsZones.Requests;

/// <summary>Request body for creating a new Private DNS Zone.</summary>
public class CreatePrivateDnsZoneRequest : PrivateDnsZoneRequestBase
{
    /// <summary>Unique identifier of the Resource Group that will own this Private DNS Zone.</summary>
    [Required, GuidValidation]
    public required Guid ResourceGroupId { get; init; }

    /// <summary>Whether this resource already exists in Azure and is not managed by this project.</summary>
    public bool IsExisting { get; init; } = false;
}
