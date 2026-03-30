using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Application.SqlServers.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;

namespace InfraFlowSculptor.Application.SqlServers.Commands.UpdateSqlServer;

/// <summary>Command to update an existing SQL Server.</summary>
public record UpdateSqlServerCommand(
    AzureResourceId Id,
    Name Name,
    Location Location,
    string Version,
    string AdministratorLogin,
    IReadOnlyList<SqlServerEnvironmentConfigData>? EnvironmentSettings = null
) : ICommand<SqlServerResult>;
