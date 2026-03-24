using ErrorOr;
using InfraFlowSculptor.Application.SqlServers.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.SqlServers.Queries;

/// <summary>Query to retrieve a SQL Server by its identifier.</summary>
public record GetSqlServerQuery(
    AzureResourceId Id
) : IRequest<ErrorOr<SqlServerResult>>;
