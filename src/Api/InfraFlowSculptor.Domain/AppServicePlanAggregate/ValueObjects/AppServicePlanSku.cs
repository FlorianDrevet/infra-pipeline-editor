using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.AppServicePlanAggregate.ValueObjects;

/// <summary>Pricing tier for an Azure App Service Plan.</summary>
public class AppServicePlanSku(AppServicePlanSku.AppServicePlanSkuEnum value)
    : EnumValueObject<AppServicePlanSku.AppServicePlanSkuEnum>(value)
{
    public enum AppServicePlanSkuEnum
    {
        F1,
        B1,
        B2,
        B3,
        S1,
        S2,
        S3,
        P1v3,
        P2v3,
        P3v3,
    }
}
