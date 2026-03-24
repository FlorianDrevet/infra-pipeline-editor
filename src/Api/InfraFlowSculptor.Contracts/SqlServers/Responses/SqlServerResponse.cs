using InfraFlowSculptor.Contracts.SqlServers.Requests;

namespace InfraFlowSculptor.Contracts.SqlServers.Responses;

/// <summary>Represents an Azure SQL Server resource.</summary>
/// <param name="Id">Unique identifier of the SQL Server.</param>
/// <param name="ResourceGroupId">Identifier of the parent Resource Group.</param>
/// <param name="Name">Display name of the SQL Server.</param>
/// <param name="Location">Azure region where the SQL Server is deployed.</param>
/// <param name="Version">SQL Server version.</param>
/// <param name="AdministratorLogin">Administrator login name.</param>
/// <param name="EnvironmentSettings">Per-environment typed configuration overrides.</param>
public record SqlServerResponse(
    Guid Id,
    Guid ResourceGroupId,
    string Name,
    string Location,
    string Version,
    string AdministratorLogin,
    IReadOnlyList<SqlServerEnvironmentConfigResponse> EnvironmentSettings
);
