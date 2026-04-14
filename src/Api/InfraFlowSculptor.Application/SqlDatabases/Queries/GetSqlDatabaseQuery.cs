using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Application.SqlDatabases.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.SqlDatabases.Queries;

/// <summary>Query to retrieve a SQL Database by its identifier.</summary>
public record GetSqlDatabaseQuery(
    AzureResourceId Id
) : IQuery<SqlDatabaseResult>;
