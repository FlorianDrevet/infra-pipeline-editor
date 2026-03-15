using System.ComponentModel.DataAnnotations;

namespace InfraFlowSculptor.Contracts.InfrastructureConfig.Requests;

public class UpdateMemberRoleRequest
{
    [Required]
    public required string NewRole { get; init; }
}
