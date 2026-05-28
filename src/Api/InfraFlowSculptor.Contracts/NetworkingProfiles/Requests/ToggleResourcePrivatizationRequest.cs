using System.ComponentModel.DataAnnotations;

namespace InfraFlowSculptor.Contracts.NetworkingProfiles.Requests;

/// <summary>Request body for toggling privatization on a resource.</summary>
public class ToggleResourcePrivatizationRequest
{
    /// <summary>Whether to privatize (<c>true</c>) or deprivatize (<c>false</c>) the resource.</summary>
    [Required]
    public required bool IsPrivatized { get; init; }
}
