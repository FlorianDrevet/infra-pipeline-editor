using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.DocumentIntelligenceAggregate.ValueObjects;

/// <summary>Pricing tier for an Azure Document Intelligence resource.</summary>
public sealed class DocumentIntelligenceSku(DocumentIntelligenceSku.Sku value) : EnumValueObject<DocumentIntelligenceSku.Sku>(value)
{
    /// <summary>Available Document Intelligence pricing tiers.</summary>
    public enum Sku
    {
        /// <summary>Free tier — 500 pages/month.</summary>
        F0,

        /// <summary>Standard tier — pay-per-use.</summary>
        S0
    }
}
