using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Domain.Common.Errors;
using MediatR;

namespace InfraFlowSculptor.Application.SqlDatabases.Commands.DeleteSqlDatabase;

/// <summary>Handles the <see cref="DeleteSqlDatabaseCommand"/> request.</summary>
public class DeleteSqlDatabaseCommandHandler(
    ISqlDatabaseRepository sqlDatabaseRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService)
    : IRequestHandler<DeleteSqlDatabaseCommand, ErrorOr<Deleted>>
{
    /// <inheritdoc />
    public async Task<ErrorOr<Deleted>> Handle(
        DeleteSqlDatabaseCommand request,
        CancellationToken cancellationToken)
    {
        var database = await sqlDatabaseRepository.GetByIdAsync(request.Id, cancellationToken);
        if (database is null)
            return Errors.SqlDatabase.NotFoundError(request.Id);

        var resourceGroup = await resourceGroupRepository.GetByIdAsync(database.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.SqlDatabase.NotFoundError(request.Id);

        var authResult = await accessService.VerifyWriteAccessAsync(resourceGroup.InfraConfigId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        await sqlDatabaseRepository.DeleteAsync(request.Id);

        return Result.Deleted;
    }
}
