using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.SqlDatabaseAggregate.ValueObjects;

/// <summary>SKU tier for an Azure SQL Database.</summary>
public class SqlDatabaseSku(SqlDatabaseSku.SqlDatabaseSkuEnum value)
    : EnumValueObject<SqlDatabaseSku.SqlDatabaseSkuEnum>(value)
{
    /// <summary>Available SQL Database SKU tiers.</summary>
    public enum SqlDatabaseSkuEnum
    {
        /// <summary>Basic tier.</summary>
        Basic,

        /// <summary>Standard tier.</summary>
        Standard,

        /// <summary>Premium tier.</summary>
        Premium,

        /// <summary>General Purpose tier (vCore-based).</summary>
        GeneralPurpose,

        /// <summary>Business Critical tier (vCore-based).</summary>
        BusinessCritical,

        /// <summary>Hyperscale tier (vCore-based).</summary>
        Hyperscale
    }
}
