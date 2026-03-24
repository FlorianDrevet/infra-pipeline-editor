using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.SqlDatabaseAggregate.ValueObjects;

/// <summary>SKU tier for an Azure SQL Database.</summary>
public class SqlDatabaseSku(SqlDatabaseSku.SqlDatabaseSkuEnum value)
    : EnumValueObject<SqlDatabaseSku.SqlDatabaseSkuEnum>(value)
{
    public enum SqlDatabaseSkuEnum
    {
        Basic,
        Standard,
        Premium,
        GeneralPurpose,
        BusinessCritical,
        Hyperscale
    }
}
