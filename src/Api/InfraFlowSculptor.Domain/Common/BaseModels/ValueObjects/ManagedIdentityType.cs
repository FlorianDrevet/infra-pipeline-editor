using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

public class ManagedIdentityType(ManagedIdentityType.IdentityTypeEnum value) : EnumValueObject<ManagedIdentityType.IdentityTypeEnum>(value)
{
    public enum IdentityTypeEnum
    {
        SystemAssigned,
        UserAssigned
    }
}
