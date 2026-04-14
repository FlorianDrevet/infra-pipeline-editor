using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Application.SqlServers.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.SqlServers.Queries;

/// <summary>Query to retrieve a SQL Server by its identifier.</summary>
public record GetSqlServerQuery(
    AzureResourceId Id
) : IQuery<SqlServerResult>;
