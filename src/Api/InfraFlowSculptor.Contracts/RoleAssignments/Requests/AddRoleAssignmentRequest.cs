using System.ComponentModel.DataAnnotations;
using InfraFlowSculptor.Contracts.ValidationAttributes;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Contracts.RoleAssignments.Requests;

public class AddRoleAssignmentRequest
{
    [Required, GuidValidation]
    public required Guid TargetResourceId { get; init; }

    [Required, EnumValidation(typeof(ManagedIdentityType.IdentityTypeEnum))]
    public required string ManagedIdentityType { get; init; }

    [Required]
    public required string RoleDefinitionId { get; init; }
}
