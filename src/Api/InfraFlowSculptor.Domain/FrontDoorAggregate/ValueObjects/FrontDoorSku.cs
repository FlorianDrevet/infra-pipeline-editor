using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.FrontDoorAggregate.ValueObjects;

/// <summary>Pricing tier for an Azure Front Door profile.</summary>
public sealed class FrontDoorSku(FrontDoorSku.SkuEnum value) : EnumValueObject<FrontDoorSku.SkuEnum>(value)
{
    /// <summary>Available Front Door pricing tiers.</summary>
    public enum SkuEnum
    {
        /// <summary>Standard tier — CDN + basic WAF.</summary>
        StandardAzureFrontDoor,
        /// <summary>Premium tier — CDN + advanced WAF + Private Link origins.</summary>
        PremiumAzureFrontDoor
    }
}
