using System.ComponentModel.DataAnnotations;
using InfraFlowSculptor.Contracts.ValidationAttributes;

namespace InfraFlowSculptor.Contracts.NetworkingProfiles.Requests;

/// <summary>Request body for toggling privatization on a resource.</summary>
public class ToggleResourcePrivatizationRequest
{
    /// <summary>The Azure resource identifier to toggle.</summary>
    [Required, GuidValidation]
    public required Guid ResourceId { get; init; }

    /// <summary>Whether to privatize (<c>true</c>) or deprivatize (<c>false</c>) the resource.</summary>
    [Required]
    public required bool IsPrivatized { get; init; }
}
