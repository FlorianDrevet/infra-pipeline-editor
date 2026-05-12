using System.ComponentModel.DataAnnotations;

namespace InfraFlowSculptor.Contracts.Projects.Requests;

/// <summary>Request to create a new project.</summary>
public class CreateProjectRequest
{
    /// <summary>Name of the project to create.</summary>
    [Required, StringLength(80, MinimumLength = 3)]
    public required string Name { get; init; }

    /// <summary>Optional description of the project.</summary>
    [StringLength(500)]
    public string? Description { get; init; }

    /// <summary>Whether this resource already exists in Azure and is not managed by this project.</summary>

    public bool IsExisting { get; init; } = false;

}
