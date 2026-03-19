using System.ComponentModel.DataAnnotations;
using InfraFlowSculptor.Contracts.ValidationAttributes;

namespace InfraFlowSculptor.Contracts.InfrastructureConfig.Requests;

/// <summary>Request body for adding a user to an Infrastructure Configuration.</summary>
public class AddMemberRequest
{
    /// <summary>The unique identifier of the user to add.</summary>
    [Required]
    [GuidValidation]
    public required Guid UserId { get; init; }

    /// <summary>Role to assign to the user. Accepted values: <c>Owner</c>, <c>Contributor</c>, <c>Reader</c>.</summary>
    [Required]
    public required string Role { get; init; }
}
