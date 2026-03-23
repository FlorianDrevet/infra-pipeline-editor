using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.AppServicePlanAggregate.ValueObjects;

/// <summary>Operating system type for an Azure App Service Plan.</summary>
public class AppServicePlanOsType(AppServicePlanOsType.AppServicePlanOsTypeEnum value)
    : EnumValueObject<AppServicePlanOsType.AppServicePlanOsTypeEnum>(value)
{
    public enum AppServicePlanOsTypeEnum
    {
        Windows,
        Linux,
    }
}
