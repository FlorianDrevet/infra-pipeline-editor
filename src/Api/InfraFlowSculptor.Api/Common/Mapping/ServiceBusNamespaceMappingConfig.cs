using InfraFlowSculptor.Application.ServiceBusNamespaces.Commands.CreateServiceBusNamespace;
using InfraFlowSculptor.Application.ServiceBusNamespaces.Commands.UpdateServiceBusNamespace;
using InfraFlowSculptor.Application.ServiceBusNamespaces.Common;
using InfraFlowSculptor.Contracts.ServiceBusNamespaces.Requests;
using InfraFlowSculptor.Contracts.ServiceBusNamespaces.Responses;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ServiceBusNamespaceAggregate;
using Mapster;

namespace InfraFlowSculptor.Api.Common.Mapping;

/// <summary>Mapster mapping configuration for the Service Bus Namespace aggregate.</summary>
public class ServiceBusNamespaceMappingConfig : IRegister
{
    /// <inheritdoc />
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateServiceBusNamespaceRequest, CreateServiceBusNamespaceCommand>()
            .Map(dest => dest.EnvironmentSettings,
                src => src.EnvironmentSettings == null
                    ? null
                    : src.EnvironmentSettings.Select(ec => new ServiceBusNamespaceEnvironmentConfigData(
                        ec.EnvironmentName,
                        ec.Sku,
                        ec.Capacity,
                        ec.ZoneRedundant,
                        ec.DisableLocalAuth,
                        ec.MinimumTlsVersion)).ToList());

        config.NewConfig<(Guid Id, UpdateServiceBusNamespaceRequest Request), UpdateServiceBusNamespaceCommand>()
            .MapWith(src => new UpdateServiceBusNamespaceCommand(
                src.Id.Adapt<AzureResourceId>(),
                src.Request.Name.Adapt<Name>(),
                src.Request.Location.Adapt<Location>(),
                src.Request.EnvironmentSettings == null
                    ? null
                    : src.Request.EnvironmentSettings.Select(ec => new ServiceBusNamespaceEnvironmentConfigData(
                        ec.EnvironmentName,
                        ec.Sku,
                        ec.Capacity,
                        ec.ZoneRedundant,
                        ec.DisableLocalAuth,
                        ec.MinimumTlsVersion)).ToList()));

        config.NewConfig<ServiceBusNamespace, ServiceBusNamespaceResult>()
            .Map(dest => dest.EnvironmentSettings,
                src => src.EnvironmentSettings.Select(es => new ServiceBusNamespaceEnvironmentConfigData(
                    es.EnvironmentName,
                    es.Sku,
                    es.Capacity,
                    es.ZoneRedundant,
                    es.DisableLocalAuth,
                    es.MinimumTlsVersion)).ToList())
            .Map(dest => dest.Queues,
                src => src.Queues.Select(q => new ServiceBusQueueResult(q.Id.Value, q.Name)).ToList())
            .Map(dest => dest.TopicSubscriptions,
                src => src.TopicSubscriptions.Select(ts => new ServiceBusTopicSubscriptionResult(ts.Id.Value, ts.TopicName, ts.SubscriptionName)).ToList());

        config.NewConfig<ServiceBusNamespaceEnvironmentConfigData, ServiceBusNamespaceEnvironmentConfigResponse>()
            .MapWith(src => new ServiceBusNamespaceEnvironmentConfigResponse(
                src.EnvironmentName,
                src.Sku,
                src.Capacity,
                src.ZoneRedundant,
                src.DisableLocalAuth,
                src.MinimumTlsVersion));

        config.NewConfig<ServiceBusQueueResult, ServiceBusQueueResponse>()
            .MapWith(src => new ServiceBusQueueResponse(src.Id, src.Name));

        config.NewConfig<ServiceBusTopicSubscriptionResult, ServiceBusTopicSubscriptionResponse>()
            .MapWith(src => new ServiceBusTopicSubscriptionResponse(src.Id, src.TopicName, src.SubscriptionName));
    }
}
