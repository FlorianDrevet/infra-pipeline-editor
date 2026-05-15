using InfraFlowSculptor.Application.NetworkSecurityGroups.Commands.CreateNetworkSecurityGroup;
using InfraFlowSculptor.Application.NetworkSecurityGroups.Commands.UpdateNetworkSecurityGroup;
using InfraFlowSculptor.Application.NetworkSecurityGroups.Common;
using InfraFlowSculptor.Contracts.NetworkSecurityGroups.Requests;
using InfraFlowSculptor.Contracts.NetworkSecurityGroups.Responses;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.NetworkSecurityGroupAggregate;
using Mapster;

namespace InfraFlowSculptor.Api.Common.Mapping;

/// <summary>Mapster configuration for Network Security Group mappings.</summary>
public sealed class NetworkSecurityGroupMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateNetworkSecurityGroupRequest, CreateNetworkSecurityGroupCommand>();

        config.NewConfig<(Guid Id, UpdateNetworkSecurityGroupRequest Request), UpdateNetworkSecurityGroupCommand>()
            .MapWith(src => new UpdateNetworkSecurityGroupCommand(
                src.Id.Adapt<AzureResourceId>(),
                src.Request.Name.Adapt<Name>(),
                src.Request.Location.Adapt<Location>()));

        config.NewConfig<NetworkSecurityGroup, NetworkSecurityGroupResult>()
            .Map(dest => dest.SecurityRules,
                src => src.SecurityRules.Select(r => new NsgRuleData(
                    r.Id.Value.ToString(),
                    r.Name,
                    r.Priority,
                    r.Direction.Value.ToString(),
                    r.Access.Value.ToString(),
                    r.Protocol.Value.ToString(),
                    r.SourceAddressPrefix,
                    r.DestinationAddressPrefix,
                    r.SourcePortRange,
                    r.DestinationPortRange)).ToList());

        config.NewConfig<NsgRuleData, NsgRuleResponse>()
            .MapWith(src => new NsgRuleResponse(
                src.Id, src.Name, src.Priority, src.Direction, src.Access, src.Protocol,
                src.SourceAddressPrefix, src.DestinationAddressPrefix, src.SourcePortRange, src.DestinationPortRange));
    }
}
