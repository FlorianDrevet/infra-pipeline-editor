using InfraFlowSculptor.Application.SqlServers.Commands.CreateSqlServer;
using InfraFlowSculptor.Application.SqlServers.Commands.UpdateSqlServer;
using InfraFlowSculptor.Application.SqlServers.Common;
using InfraFlowSculptor.Contracts.SqlServers.Requests;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.SqlServerAggregate;
using Mapster;

namespace InfraFlowSculptor.Api.Common.Mapping;

/// <summary>Mapster mapping configuration for the SQL Server feature.</summary>
public sealed class SqlServerMappingConfig : IRegister
{
    /// <inheritdoc />
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateSqlServerRequest, CreateSqlServerCommand>()
            .Map(dest => dest.EnvironmentSettings,
                src => src.EnvironmentSettings == null
                    ? null
                    : src.EnvironmentSettings.Select(ec => new SqlServerEnvironmentConfigData(
                        ec.EnvironmentName, ec.MinimalTlsVersion)).ToList());

        config.NewConfig<(Guid Id, UpdateSqlServerRequest Request), UpdateSqlServerCommand>()
            .MapWith(src => new UpdateSqlServerCommand(
                new AzureResourceId(src.Id),
                src.Request.Name.Adapt<Name>(),
                src.Request.Location.Adapt<Location>(),
                src.Request.Version,
                src.Request.AdministratorLogin,
                src.Request.EnvironmentSettings == null
                    ? null
                    : src.Request.EnvironmentSettings.Select(ec => new SqlServerEnvironmentConfigData(
                        ec.EnvironmentName, ec.MinimalTlsVersion)).ToList()));

        config.NewConfig<SqlServer, SqlServerResult>()
            .Map(dest => dest.EnvironmentSettings,
                src => src.EnvironmentSettings.Select(es => new SqlServerEnvironmentConfigData(
                    es.EnvironmentName,
                    es.MinimalTlsVersion)).ToList())
            .Map(dest => dest.Version, src => src.Version.Value.ToString())
            .Map(dest => dest.AdministratorLogin, src => src.AdministratorLogin);

        config.NewConfig<SqlServerEnvironmentConfigData, SqlServerEnvironmentConfigResponse>()
            .MapWith(src => new SqlServerEnvironmentConfigResponse(
                src.EnvironmentName, src.MinimalTlsVersion));
    }
}
