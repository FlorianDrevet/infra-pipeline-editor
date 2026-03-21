using System.ComponentModel.DataAnnotations;

namespace InfraFlowSculptor.Contracts.Projects.Requests;

/// <summary>Request body for creating a new project.</summary>
public class CreateProjectRequest
{
    /// <summary>Human-readable name for the new project.</summary>
    [Required]
    public required string Name { get; init; }

    /// <summary>Optional description for the project.</summary>
    public string? Description { get; init; }
}
