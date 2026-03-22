using System.ComponentModel.DataAnnotations;

namespace InfraFlowSculptor.Contracts.InfrastructureConfig.Requests;

/// <summary>Request body for toggling project-level inheritance for environments and naming conventions.</summary>
public class SetInheritanceRequest
{
    /// <summary>When true, this configuration inherits environments from the parent project.</summary>
    [Required]
    public required bool UseProjectEnvironments { get; init; }

    /// <summary>When true, this configuration inherits naming conventions from the parent project.</summary>
    [Required]
    public required bool UseProjectNamingConventions { get; init; }
}
