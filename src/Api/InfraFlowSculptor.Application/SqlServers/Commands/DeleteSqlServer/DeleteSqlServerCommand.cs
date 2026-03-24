using ErrorOr;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.SqlServers.Commands.DeleteSqlServer;

/// <summary>Command to delete a SQL Server.</summary>
public record DeleteSqlServerCommand(
    AzureResourceId Id
) : IRequest<ErrorOr<Deleted>>;
