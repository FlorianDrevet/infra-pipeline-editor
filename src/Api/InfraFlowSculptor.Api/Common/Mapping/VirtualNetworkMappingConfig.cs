using InfraFlowSculptor.Application.VirtualNetworks.Commands.CreateVirtualNetwork;
using InfraFlowSculptor.Application.VirtualNetworks.Commands.UpdateVirtualNetwork;
using InfraFlowSculptor.Application.VirtualNetworks.Common;
using InfraFlowSculptor.Contracts.VirtualNetworks.Requests;
using InfraFlowSculptor.Contracts.VirtualNetworks.Responses;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.VirtualNetworkAggregate;
using Mapster;

namespace InfraFlowSculptor.Api.Common.Mapping;

/// <summary>Mapster configuration for Virtual Network mappings.</summary>
public sealed class VirtualNetworkMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateVirtualNetworkRequest, CreateVirtualNetworkCommand>()
            .Map(dest => dest.EnvironmentSettings,
                src => src.EnvironmentSettings == null
                    ? null
                    : src.EnvironmentSettings.Select(ec => new VirtualNetworkEnvironmentConfigData(
                        ec.EnvironmentName, ec.AddressSpaces, ec.DnsServers)).ToList());

        config.NewConfig<(Guid Id, UpdateVirtualNetworkRequest Request), UpdateVirtualNetworkCommand>()
            .MapWith(src => new UpdateVirtualNetworkCommand(
                src.Id.Adapt<AzureResourceId>(),
                src.Request.Name.Adapt<Name>(),
                src.Request.Location.Adapt<Location>(),
                src.Request.EnableDdosProtection,
                src.Request.EnvironmentSettings == null
                    ? null
                    : src.Request.EnvironmentSettings.Select(ec => new VirtualNetworkEnvironmentConfigData(
                        ec.EnvironmentName, ec.AddressSpaces, ec.DnsServers)).ToList()));

        config.NewConfig<VirtualNetwork, VirtualNetworkResult>()
            .Map(dest => dest.Subnets,
                src => src.Subnets.Select(s => new SubnetData(
                    s.Id.Value.ToString(),
                    s.Name.Value,
                    s.AddressPrefix,
                    s.Delegation != null ? s.Delegation.Value.ToString() : null,
                    s.ServiceEndpoints.Count > 0 ? s.ServiceEndpoints : null,
                    s.PrivateEndpointNetworkPolicies.Value.ToString(),
                    s.NsgId != null ? s.NsgId.Value.ToString() : null)).ToList())
            .Map(dest => dest.EnvironmentSettings,
                src => src.EnvironmentSettings.Select(es => new VirtualNetworkEnvironmentConfigData(
                    es.EnvironmentName,
                    es.AddressSpaces,
                    es.DnsServers)).ToList());

        config.NewConfig<SubnetData, SubnetResponse>()
            .MapWith(src => new SubnetResponse(
                src.Id, src.Name, src.AddressPrefix, src.Delegation, src.ServiceEndpoints, src.PrivateEndpointNetworkPolicies, src.NsgId));

        config.NewConfig<VirtualNetworkEnvironmentConfigData, VirtualNetworkEnvironmentConfigResponse>()
            .MapWith(src => new VirtualNetworkEnvironmentConfigResponse(
                src.EnvironmentName, src.AddressSpaces, src.DnsServers));
    }
}
