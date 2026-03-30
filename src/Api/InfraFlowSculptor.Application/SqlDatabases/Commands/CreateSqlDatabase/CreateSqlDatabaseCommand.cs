using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.SqlDatabases.Common;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using ErrorOr;

namespace InfraFlowSculptor.Application.SqlDatabases.Commands.CreateSqlDatabase;

/// <summary>Command to create a new SQL Database inside a Resource Group.</summary>
public record CreateSqlDatabaseCommand(
    ResourceGroupId ResourceGroupId,
    Name Name,
    Location Location,
    Guid SqlServerId,
    string Collation,
    IReadOnlyList<SqlDatabaseEnvironmentConfigData>? EnvironmentSettings = null
) : ICommand<SqlDatabaseResult>;
