using System.ComponentModel.DataAnnotations;

namespace InfraFlowSculptor.Contracts.Projects.Requests;

/// <summary>Request body for updating a project member's role.</summary>
public class UpdateProjectMemberRoleRequest
{
    /// <summary>The new role to assign (Owner, Contributor, Reader).</summary>
    [Required]
    public required string NewRole { get; init; }
}
