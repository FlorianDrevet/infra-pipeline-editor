using InfraFlowSculptor.Application.PersonalAccessTokens.Commands.CreatePersonalAccessToken;
using InfraFlowSculptor.Application.PersonalAccessTokens.Common;
using InfraFlowSculptor.Contracts.PersonalAccessTokens.Requests;
using InfraFlowSculptor.Contracts.PersonalAccessTokens.Responses;
using Mapster;

namespace InfraFlowSculptor.Api.Common.Mapping;

/// <summary>
/// Mapster configuration for personal access token request/response mappings.
/// </summary>
public sealed class PersonalAccessTokenMappingConfig : IRegister
{
    /// <inheritdoc />
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreatePersonalAccessTokenRequest, CreatePersonalAccessTokenCommand>();

        config.NewConfig<PersonalAccessTokenResult, PersonalAccessTokenResponse>()
            .Map(dest => dest.Id, src => src.Id.Value.ToString());
    }
}
