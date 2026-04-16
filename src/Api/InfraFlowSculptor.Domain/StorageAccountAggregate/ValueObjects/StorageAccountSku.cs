using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;

/// <summary>Performance/replication tier for an Azure Storage Account.</summary>
public sealed class StorageAccountSku(StorageAccountSku.Sku value) : EnumValueObject<StorageAccountSku.Sku>(value)
{
    /// <summary>Available Storage Account SKU tiers.</summary>
    public enum Sku
    {
        Standard_LRS,
        Standard_GRS,
        Standard_RAGRS,
        Standard_ZRS,
        Premium_LRS,
        Premium_ZRS
    }
}
