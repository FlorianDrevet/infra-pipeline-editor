using InfraFlowSculptor.Application.UserAssignedIdentities.Commands.CreateUserAssignedIdentity;
using InfraFlowSculptor.Application.UserAssignedIdentities.Commands.UpdateUserAssignedIdentity;
using InfraFlowSculptor.Application.UserAssignedIdentities.Common;
using InfraFlowSculptor.Contracts.UserAssignedIdentities.Requests;
using InfraFlowSculptor.Contracts.UserAssignedIdentities.Responses;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.UserAssignedIdentityAggregate;
using Mapster;

namespace InfraFlowSculptor.Api.Common.Mapping;

/// <summary>
/// Mapster type adapter configuration for user-assigned identity request/command/result/response mappings.
/// </summary>
public sealed class UserAssignedIdentityMappingConfig : IRegister
{
    /// <inheritdoc />
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateUserAssignedIdentityRequest, CreateUserAssignedIdentityCommand>();

        config.NewConfig<(Guid Id, UpdateUserAssignedIdentityRequest Request), UpdateUserAssignedIdentityCommand>()
            .MapWith(src => new UpdateUserAssignedIdentityCommand(
                src.Id.Adapt<AzureResourceId>(),
                src.Request.Name.Adapt<Name>(),
                src.Request.Location.Adapt<Location>()));

        config.NewConfig<UserAssignedIdentity, UserAssignedIdentityResult>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.ResourceGroupId, src => src.ResourceGroupId)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Location, src => src.Location);

        config.NewConfig<UserAssignedIdentityResult, UserAssignedIdentityResponse>()
            .Map(dest => dest.Id, src => src.Id.Value.ToString())
            .Map(dest => dest.ResourceGroupId, src => src.ResourceGroupId.Value.ToString())
            .Map(dest => dest.Name, src => src.Name.Value)
            .Map(dest => dest.Location, src => src.Location.Value.ToString());
    }
}
