using System.ComponentModel.DataAnnotations;

namespace InfraFlowSculptor.Contracts.Projects.Requests;

/// <summary>Request to add a member to a project.</summary>
public class AddProjectMemberRequest
{
    /// <summary>Identifier of the user to add.</summary>
    [Required]
    public required Guid UserId { get; init; }

    /// <summary>Role to assign (Owner, Contributor, Reader).</summary>
    [Required]
    public required string Role { get; init; }
}
