using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Application.SqlDatabases.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;

namespace InfraFlowSculptor.Application.SqlDatabases.Commands.UpdateSqlDatabase;

/// <summary>Command to update an existing SQL Database.</summary>
public record UpdateSqlDatabaseCommand(
    AzureResourceId Id,
    Name Name,
    Location Location,
    Guid SqlServerId,
    string Collation,
    IReadOnlyList<SqlDatabaseEnvironmentConfigData>? EnvironmentSettings = null
) : ICommand<SqlDatabaseResult>;
