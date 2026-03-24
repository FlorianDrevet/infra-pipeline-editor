using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.StorageAccountAggregate.ValueObjects;

public class StorageAccountSku(StorageAccountSku.Sku value) : EnumValueObject<StorageAccountSku.Sku>(value)
{
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
