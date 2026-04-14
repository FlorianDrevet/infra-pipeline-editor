using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.SqlServers.Commands.DeleteSqlServer;

/// <summary>Command to delete a SQL Server.</summary>
public record DeleteSqlServerCommand(
    AzureResourceId Id
) : ICommand<Deleted>;
