using System.ComponentModel.DataAnnotations;

namespace InfraFlowSculptor.Contracts.Projects.Requests;

/// <summary>Request to update a project member's role.</summary>
public class UpdateProjectMemberRoleRequest
{
    /// <summary>New role to assign (Owner, Contributor, Reader).</summary>
    [Required]
    public required string NewRole { get; init; }
}
