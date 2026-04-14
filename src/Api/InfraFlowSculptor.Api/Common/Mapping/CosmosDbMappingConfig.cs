using InfraFlowSculptor.Application.CosmosDbs.Commands.CreateCosmosDb;
using InfraFlowSculptor.Application.CosmosDbs.Commands.UpdateCosmosDb;
using InfraFlowSculptor.Application.CosmosDbs.Common;
using InfraFlowSculptor.Contracts.CosmosDbs.Requests;
using InfraFlowSculptor.Domain.CosmosDbAggregate;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using Mapster;

namespace InfraFlowSculptor.Api.Common.Mapping;

/// <summary>Mapster mapping configuration for the Cosmos DB aggregate.</summary>
public sealed class CosmosDbMappingConfig : IRegister
{
    /// <inheritdoc />
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateCosmosDbRequest, CreateCosmosDbCommand>()
            .Map(dest => dest.EnvironmentSettings,
                src => src.EnvironmentSettings == null
                    ? null
                    : src.EnvironmentSettings.Select(ec => new CosmosDbEnvironmentConfigData(
                        ec.EnvironmentName,
                        ec.DatabaseApiType,
                        ec.ConsistencyLevel,
                        ec.MaxStalenessPrefix,
                        ec.MaxIntervalInSeconds,
                        ec.EnableAutomaticFailover,
                        ec.EnableMultipleWriteLocations,
                        ec.BackupPolicyType,
                        ec.EnableFreeTier)).ToList());

        config.NewConfig<(Guid Id, UpdateCosmosDbRequest Request), UpdateCosmosDbCommand>()
            .MapWith(src => new UpdateCosmosDbCommand(
                src.Id.Adapt<AzureResourceId>(),
                src.Request.Name.Adapt<Name>(),
                src.Request.Location.Adapt<Location>(),
                src.Request.EnvironmentSettings == null
                    ? null
                    : src.Request.EnvironmentSettings.Select(ec => new CosmosDbEnvironmentConfigData(
                        ec.EnvironmentName,
                        ec.DatabaseApiType,
                        ec.ConsistencyLevel,
                        ec.MaxStalenessPrefix,
                        ec.MaxIntervalInSeconds,
                        ec.EnableAutomaticFailover,
                        ec.EnableMultipleWriteLocations,
                        ec.BackupPolicyType,
                        ec.EnableFreeTier)).ToList()));

        config.NewConfig<CosmosDb, CosmosDbResult>()
            .Map(dest => dest.EnvironmentSettings,
                src => src.EnvironmentSettings.Select(es => new CosmosDbEnvironmentConfigData(
                    es.EnvironmentName,
                    es.DatabaseApiType,
                    es.ConsistencyLevel,
                    es.MaxStalenessPrefix,
                    es.MaxIntervalInSeconds,
                    es.EnableAutomaticFailover,
                    es.EnableMultipleWriteLocations,
                    es.BackupPolicyType,
                    es.EnableFreeTier)).ToList());

        config.NewConfig<CosmosDbEnvironmentConfigData, CosmosDbEnvironmentConfigResponse>()
            .MapWith(src => new CosmosDbEnvironmentConfigResponse(
                src.EnvironmentName,
                src.DatabaseApiType,
                src.ConsistencyLevel,
                src.MaxStalenessPrefix,
                src.MaxIntervalInSeconds,
                src.EnableAutomaticFailover,
                src.EnableMultipleWriteLocations,
                src.BackupPolicyType,
                src.EnableFreeTier));
    }
}
