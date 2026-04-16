using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.AppServicePlanAggregate.ValueObjects;

/// <summary>Pricing tier for an Azure App Service Plan.</summary>
public sealed class AppServicePlanSku(AppServicePlanSku.AppServicePlanSkuEnum value)
    : EnumValueObject<AppServicePlanSku.AppServicePlanSkuEnum>(value)
{
    /// <summary>Available pricing tier values.</summary>
    public enum AppServicePlanSkuEnum
    {
        /// <summary>Free tier.</summary>
        F1,

        /// <summary>Basic tier — small.</summary>
        B1,

        /// <summary>Basic tier — medium.</summary>
        B2,

        /// <summary>Basic tier — large.</summary>
        B3,

        /// <summary>Standard tier — small.</summary>
        S1,

        /// <summary>Standard tier — medium.</summary>
        S2,

        /// <summary>Standard tier — large.</summary>
        S3,

        /// <summary>Premium V3 tier — small.</summary>
        P1v3,

        /// <summary>Premium V3 tier — medium.</summary>
        P2v3,

        /// <summary>Premium V3 tier — large.</summary>
        P3v3,
    }
}
