using InfraFlowSculptor.Application.SqlDatabases.Commands.CreateSqlDatabase;
using InfraFlowSculptor.Application.SqlDatabases.Commands.UpdateSqlDatabase;
using InfraFlowSculptor.Application.SqlDatabases.Common;
using InfraFlowSculptor.Contracts.SqlDatabases.Requests;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.SqlDatabaseAggregate;
using Mapster;

namespace InfraFlowSculptor.Api.Common.Mapping;

/// <summary>Mapster mapping configuration for the SQL Database feature.</summary>
public sealed class SqlDatabaseMappingConfig : IRegister
{
    /// <inheritdoc />
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateSqlDatabaseRequest, CreateSqlDatabaseCommand>()
            .Map(dest => dest.EnvironmentSettings,
                src => src.EnvironmentSettings == null
                    ? null
                    : src.EnvironmentSettings.Select(ec => new SqlDatabaseEnvironmentConfigData(
                        ec.EnvironmentName, ec.Sku, ec.MaxSizeGb, ec.ZoneRedundant)).ToList());

        config.NewConfig<(Guid Id, UpdateSqlDatabaseRequest Request), UpdateSqlDatabaseCommand>()
            .MapWith(src => new UpdateSqlDatabaseCommand(
                new AzureResourceId(src.Id),
                src.Request.Name.Adapt<Name>(),
                src.Request.Location.Adapt<Location>(),
                src.Request.SqlServerId,
                src.Request.Collation,
                src.Request.EnvironmentSettings == null
                    ? null
                    : src.Request.EnvironmentSettings.Select(ec => new SqlDatabaseEnvironmentConfigData(
                        ec.EnvironmentName, ec.Sku, ec.MaxSizeGb, ec.ZoneRedundant)).ToList()));

        config.NewConfig<SqlDatabase, SqlDatabaseResult>()
            .Map(dest => dest.EnvironmentSettings,
                src => src.EnvironmentSettings.Select(es => new SqlDatabaseEnvironmentConfigData(
                    es.EnvironmentName,
                    (object?)es.Sku != null ? es.Sku.Value.ToString() : null,
                    es.MaxSizeGb,
                    es.ZoneRedundant)).ToList())
            .Map(dest => dest.Collation, src => src.Collation)
            .Map(dest => dest.SqlServerId, src => src.SqlServerId.Value);

        config.NewConfig<SqlDatabaseEnvironmentConfigData, SqlDatabaseEnvironmentConfigResponse>()
            .MapWith(src => new SqlDatabaseEnvironmentConfigResponse(
                src.EnvironmentName, src.Sku, src.MaxSizeGb, src.ZoneRedundant));
    }
}
