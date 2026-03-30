using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.SqlDatabases.Commands.DeleteSqlDatabase;

/// <summary>Command to delete a SQL Database.</summary>
public record DeleteSqlDatabaseCommand(
    AzureResourceId Id
) : ICommand<Deleted>;
