using System.ComponentModel.DataAnnotations;
using InfraFlowSculptor.Contracts.ValidationAttributes;

namespace InfraFlowSculptor.Contracts.InfrastructureConfig.Requests;

public class AddMemberRequest
{
    [Required]
    [GuidValidation]
    public required Guid UserId { get; init; }

    [Required]
    public required string Role { get; init; }
}
