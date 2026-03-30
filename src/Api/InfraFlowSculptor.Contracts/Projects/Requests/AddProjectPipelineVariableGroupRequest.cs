using System.ComponentModel.DataAnnotations;

namespace InfraFlowSculptor.Contracts.Projects.Requests;

/// <summary>Request body for adding a pipeline variable group to a project (shared across all configurations).</summary>
public class AddProjectPipelineVariableGroupRequest
{
    /// <summary>Name of the Azure DevOps Variable Group (Library).</summary>
    [Required]
    public required string GroupName { get; init; }
}
