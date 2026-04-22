using InfraFlowSculptor.Contracts.SqlDatabases.Requests;

namespace InfraFlowSculptor.Contracts.SqlDatabases.Responses;

/// <summary>Represents an Azure SQL Database resource.</summary>
/// <param name="Id">Unique identifier of the SQL Database.</param>
/// <param name="ResourceGroupId">Identifier of the parent Resource Group.</param>
/// <param name="Name">Display name of the SQL Database.</param>
/// <param name="Location">Azure region where the SQL Database is deployed.</param>
/// <param name="SqlServerId">Identifier of the hosting SQL Server.</param>
/// <param name="Collation">Database collation.</param>
/// <param name="EnvironmentSettings">Per-environment typed configuration overrides.</param>
public record SqlDatabaseResponse(
    string Id,
    string ResourceGroupId,
    string Name,
    string Location,
    string SqlServerId,
    string Collation,
    IReadOnlyList<SqlDatabaseEnvironmentConfigResponse> EnvironmentSettings,

    bool IsExisting = false

);
