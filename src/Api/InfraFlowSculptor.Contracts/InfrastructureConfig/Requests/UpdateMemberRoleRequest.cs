using System.ComponentModel.DataAnnotations;

namespace InfraFlowSculptor.Contracts.InfrastructureConfig.Requests;

/// <summary>Request body for changing the role of an existing member.</summary>
public class UpdateMemberRoleRequest
{
    /// <summary>New role to assign to the member. Accepted values: <c>Owner</c>, <c>Contributor</c>, <c>Reader</c>.</summary>
    [Required]
    public required string NewRole { get; init; }
}
