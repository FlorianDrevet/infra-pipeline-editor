using System.ComponentModel.DataAnnotations;
using InfraFlowSculptor.Contracts.ValidationAttributes;
using InfraFlowSculptor.Domain.Common.ValueObjects;

namespace InfraFlowSculptor.Contracts.ResourceGroups.Requests;

public class CreateResourceGroupRequest
{
    [Required, GuidValidation] 
    public required Guid InfraConfigId { get; init; }

    [Required] public required string Name { get; init; }

    [Required, EnumValidation(typeof(Location.LocationEnum))]
    public required string Location { get; init; }
}
