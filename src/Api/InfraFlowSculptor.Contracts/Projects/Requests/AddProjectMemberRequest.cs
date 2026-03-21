using System.ComponentModel.DataAnnotations;

namespace InfraFlowSculptor.Contracts.Projects.Requests;

/// <summary>Request body for adding a member to a project.</summary>
public class AddProjectMemberRequest
{
    /// <summary>The user to add as a member.</summary>
    [Required]
    public required Guid UserId { get; init; }

    /// <summary>The role to assign (Owner, Contributor, Reader).</summary>
    [Required]
    public required string Role { get; init; }
}
