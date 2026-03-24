using InfraFlowSculptor.Application.SqlServers.Common;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using MediatR;
using ErrorOr;

namespace InfraFlowSculptor.Application.SqlServers.Commands.CreateSqlServer;

/// <summary>Command to create a new SQL Server inside a Resource Group.</summary>
public record CreateSqlServerCommand(
    ResourceGroupId ResourceGroupId,
    Name Name,
    Location Location,
    string Version,
    string AdministratorLogin,
    IReadOnlyList<SqlServerEnvironmentConfigData>? EnvironmentSettings = null
) : IRequest<ErrorOr<SqlServerResult>>;
