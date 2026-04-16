using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.AppServicePlanAggregate.ValueObjects;

/// <summary>Operating system type for an Azure App Service Plan.</summary>
public sealed class AppServicePlanOsType(AppServicePlanOsType.AppServicePlanOsTypeEnum value)
    : EnumValueObject<AppServicePlanOsType.AppServicePlanOsTypeEnum>(value)
{
    /// <summary>Available operating system types.</summary>
    public enum AppServicePlanOsTypeEnum
    {
        /// <summary>Windows operating system.</summary>
        Windows,

        /// <summary>Linux operating system.</summary>
        Linux,
    }
}
