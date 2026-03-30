using System.ComponentModel.DataAnnotations;

namespace InfraFlowSculptor.Contracts.Projects.Requests;

/// <summary>Request body for adding a variable mapping to a project-level pipeline variable group.</summary>
public class AddProjectPipelineVariableMappingRequest
{
    /// <summary>Variable name in the Azure DevOps Library.</summary>
    [Required]
    public required string PipelineVariableName { get; init; }

    /// <summary>Target Bicep parameter name in main.bicep.</summary>
    [Required]
    public required string BicepParameterName { get; init; }
}
