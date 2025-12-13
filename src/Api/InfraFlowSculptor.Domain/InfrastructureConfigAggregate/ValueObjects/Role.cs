using Shared.Domain.Domain.Models;
using Shared.Domain.Models;

namespace InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;

public class Role(Role.RoleEnum value) : EnumValueObject<Role.RoleEnum>(value)
{
    public enum RoleEnum
    {
        Owner,
        Contributor,
        Reader,
    }
}