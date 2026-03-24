using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.SqlDatabases.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.SqlDatabaseAggregate.ValueObjects;
using MapsterMapper;
using MediatR;

namespace InfraFlowSculptor.Application.SqlDatabases.Commands.UpdateSqlDatabase;

/// <summary>Handles the <see cref="UpdateSqlDatabaseCommand"/> request.</summary>
public class UpdateSqlDatabaseCommandHandler(
    ISqlDatabaseRepository sqlDatabaseRepository,
    ISqlServerRepository sqlServerRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : IRequestHandler<UpdateSqlDatabaseCommand, ErrorOr<SqlDatabaseResult>>
{
    /// <inheritdoc />
    public async Task<ErrorOr<SqlDatabaseResult>> Handle(
        UpdateSqlDatabaseCommand request,
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

        // Verify the SQL Server exists
        var sqlServerId = new AzureResourceId(request.SqlServerId);
        var sqlServer = await sqlServerRepository.GetByIdAsync(sqlServerId, cancellationToken);
        if (sqlServer is null)
            return Errors.SqlServer.NotFoundError(sqlServerId);

        database.Update(request.Name, request.Location, sqlServerId, request.Collation);

        if (request.EnvironmentSettings is not null)
            database.SetAllEnvironmentSettings(
                request.EnvironmentSettings
                    .Select(ec => (ec.EnvironmentName,
                        ec.Sku is not null
                            ? new SqlDatabaseSku(Enum.Parse<SqlDatabaseSku.SqlDatabaseSkuEnum>(ec.Sku))
                            : (SqlDatabaseSku?)null,
                        ec.MaxSizeGb,
                        ec.ZoneRedundant))
                    .ToList());

        var updated = await sqlDatabaseRepository.UpdateAsync(database);

        return mapper.Map<SqlDatabaseResult>(updated);
    }
}
