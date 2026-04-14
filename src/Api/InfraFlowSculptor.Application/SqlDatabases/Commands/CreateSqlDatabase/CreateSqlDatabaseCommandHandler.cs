using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.Common.Interfaces.Persistence;
using InfraFlowSculptor.Application.SqlDatabases.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Errors;
using InfraFlowSculptor.Domain.SqlDatabaseAggregate;
using InfraFlowSculptor.Domain.SqlDatabaseAggregate.ValueObjects;
using MapsterMapper;
using MediatR;
using ErrorOr;

namespace InfraFlowSculptor.Application.SqlDatabases.Commands.CreateSqlDatabase;

/// <summary>Handles the <see cref="CreateSqlDatabaseCommand"/> request.</summary>
public class CreateSqlDatabaseCommandHandler(
    ISqlDatabaseRepository sqlDatabaseRepository,
    ISqlServerRepository sqlServerRepository,
    IResourceGroupRepository resourceGroupRepository,
    IInfraConfigAccessService accessService,
    IMapper mapper)
    : ICommandHandler<CreateSqlDatabaseCommand, SqlDatabaseResult>
{
    /// <inheritdoc />
    public async Task<ErrorOr<SqlDatabaseResult>> Handle(
        CreateSqlDatabaseCommand request,
        CancellationToken cancellationToken)
    {
        var resourceGroup = await resourceGroupRepository.GetByIdAsync(request.ResourceGroupId, cancellationToken);
        if (resourceGroup is null)
            return Errors.ResourceGroup.NotFound(request.ResourceGroupId);

        var authResult = await accessService.VerifyWriteAccessAsync(resourceGroup.InfraConfigId, cancellationToken);
        if (authResult.IsError)
            return authResult.Errors;

        // Verify the SQL Server exists
        var sqlServerId = new AzureResourceId(request.SqlServerId);
        var sqlServer = await sqlServerRepository.GetByIdAsync(sqlServerId, cancellationToken);
        if (sqlServer is null)
            return Errors.SqlServer.NotFoundError(sqlServerId);

        var database = SqlDatabase.Create(
            request.ResourceGroupId,
            request.Name,
            request.Location,
            sqlServerId,
            request.Collation,
            request.EnvironmentSettings?
                .Select(ec => (ec.EnvironmentName,
                    ec.Sku is not null
                        ? new SqlDatabaseSku(Enum.Parse<SqlDatabaseSku.SqlDatabaseSkuEnum>(ec.Sku))
                        : (SqlDatabaseSku?)null,
                    ec.MaxSizeGb,
                    ec.ZoneRedundant))
                .ToList());

        var saved = await sqlDatabaseRepository.AddAsync(database);

        return mapper.Map<SqlDatabaseResult>(saved);
    }
}
