using InfraFlowSculptor.Application.EventHubNamespaces.Commands.CreateEventHubNamespace;
using InfraFlowSculptor.Application.EventHubNamespaces.Commands.UpdateEventHubNamespace;
using InfraFlowSculptor.Application.EventHubNamespaces.Common;
using InfraFlowSculptor.Contracts.EventHubNamespaces.Requests;
using InfraFlowSculptor.Contracts.EventHubNamespaces.Responses;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.EventHubNamespaceAggregate;
using Mapster;

namespace InfraFlowSculptor.Api.Common.Mapping;

/// <summary>Mapster mapping configuration for the Event Hub Namespace aggregate.</summary>
public sealed class EventHubNamespaceMappingConfig : IRegister
{
    /// <inheritdoc />
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateEventHubNamespaceRequest, CreateEventHubNamespaceCommand>()
            .Map(dest => dest.EnvironmentSettings,
                src => src.EnvironmentSettings == null
                    ? null
                    : src.EnvironmentSettings.Select(ec => new EventHubNamespaceEnvironmentConfigData(
                        ec.EnvironmentName,
                        ec.Sku,
                        ec.Capacity,
                        ec.ZoneRedundant,
                        ec.DisableLocalAuth,
                        ec.MinimumTlsVersion,
                        ec.AutoInflateEnabled,
                        ec.MaxThroughputUnits)).ToList());

        config.NewConfig<(Guid Id, UpdateEventHubNamespaceRequest Request), UpdateEventHubNamespaceCommand>()
            .MapWith(src => new UpdateEventHubNamespaceCommand(
                src.Id.Adapt<AzureResourceId>(),
                src.Request.Name.Adapt<Name>(),
                src.Request.Location.Adapt<Location>(),
                src.Request.EnvironmentSettings == null
                    ? null
                    : src.Request.EnvironmentSettings.Select(ec => new EventHubNamespaceEnvironmentConfigData(
                        ec.EnvironmentName,
                        ec.Sku,
                        ec.Capacity,
                        ec.ZoneRedundant,
                        ec.DisableLocalAuth,
                        ec.MinimumTlsVersion,
                        ec.AutoInflateEnabled,
                        ec.MaxThroughputUnits)).ToList()));

        config.NewConfig<EventHubNamespace, EventHubNamespaceResult>()
            .Map(dest => dest.EnvironmentSettings,
                src => src.EnvironmentSettings.Select(es => new EventHubNamespaceEnvironmentConfigData(
                    es.EnvironmentName,
                    es.Sku,
                    es.Capacity,
                    es.ZoneRedundant,
                    es.DisableLocalAuth,
                    es.MinimumTlsVersion,
                    es.AutoInflateEnabled,
                    es.MaxThroughputUnits)).ToList())
            .Map(dest => dest.EventHubs,
                src => src.EventHubs.Select(e => new EventHubResult(e.Id.Value, e.Name)).ToList())
            .Map(dest => dest.ConsumerGroups,
                src => src.ConsumerGroups.Select(cg => new EventHubConsumerGroupResult(cg.Id.Value, cg.EventHubName, cg.ConsumerGroupName)).ToList());

        config.NewConfig<EventHubNamespaceEnvironmentConfigData, EventHubNamespaceEnvironmentConfigResponse>()
            .MapWith(src => new EventHubNamespaceEnvironmentConfigResponse(
                src.EnvironmentName,
                src.Sku,
                src.Capacity,
                src.ZoneRedundant,
                src.DisableLocalAuth,
                src.MinimumTlsVersion,
                src.AutoInflateEnabled,
                src.MaxThroughputUnits));

        config.NewConfig<EventHubResult, EventHubResponse>()
            .MapWith(src => new EventHubResponse(src.Id.ToString(), src.Name));

        config.NewConfig<EventHubConsumerGroupResult, EventHubConsumerGroupResponse>()
            .MapWith(src => new EventHubConsumerGroupResponse(src.Id.ToString(), src.EventHubName, src.ConsumerGroupName));
    }
}
