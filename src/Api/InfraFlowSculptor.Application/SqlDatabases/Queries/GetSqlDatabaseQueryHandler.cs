using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.SqlDatabases.Common;
using InfraFlowSculptor.Domain.Common.Errors;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.SqlDatabases.Queries;

/// <summary>Handles the <see cref="GetSqlDatabaseQuery"/> request.</summary>
public class GetSqlDatabaseQueryHandler(
    ISqlDatabaseRepository sqlDatabaseRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : IRequestHandler<GetSqlDatabaseQuery, ErrorOr<SqlDatabaseResult>>
{
    /// <inheritdoc />
    public async Task<ErrorOr<SqlDatabaseResult>> Handle(
        GetSqlDatabaseQuery query,
        CancellationToken cancellationToken)
    {
        var database = await sqlDatabaseRepository.GetByIdAsync(query.Id, cancellationToken);
        if (database is null)
            return Errors.SqlDatabase.NotFoundError(query.Id);

        var resourceGroup = await resourceGroupRepository.GetByIdAsync(database.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.SqlDatabase.NotFoundError(query.Id);

        var authResult = await accessService.VerifyReadAccessAsync(resourceGroup.InfraConfigId, cancellationToken);
        if (authResult.IsError)
            return Errors.SqlDatabase.NotFoundError(query.Id);

        return mapper.Map<SqlDatabaseResult>(database);
    }
}
