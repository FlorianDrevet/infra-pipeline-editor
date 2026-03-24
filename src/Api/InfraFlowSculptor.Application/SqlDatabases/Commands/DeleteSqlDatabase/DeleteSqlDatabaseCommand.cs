using ErrorOr;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.SqlDatabases.Commands.DeleteSqlDatabase;

/// <summary>Command to delete a SQL Database.</summary>
public record DeleteSqlDatabaseCommand(
    AzureResourceId Id
) : IRequest<ErrorOr<Deleted>>;
