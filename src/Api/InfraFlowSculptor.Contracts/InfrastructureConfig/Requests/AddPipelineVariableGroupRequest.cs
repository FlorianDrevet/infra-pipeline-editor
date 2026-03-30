using System.ComponentModel.DataAnnotations;

namespace InfraFlowSculptor.Contracts.InfrastructureConfig.Requests;

/// <summary>Request body for adding a pipeline variable group to an infrastructure configuration.</summary>
public class AddPipelineVariableGroupRequest
{
    /// <summary>Name of the Azure DevOps Variable Group (Library).</summary>
    [Required]
    public required string GroupName { get; init; }
}
