using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.SqlServerAggregate.ValueObjects;

/// <summary>Represents the SQL Server version (e.g., "12.0" for SQL Server 2014+).</summary>
public sealed class SqlServerVersion(SqlServerVersion.SqlServerVersionEnum value)
    : EnumValueObject<SqlServerVersion.SqlServerVersionEnum>(value)
{
    public enum SqlServerVersionEnum
    {
        /// <summary>SQL Server version 12.0 (default for Azure SQL).</summary>
        V12
    }
}
