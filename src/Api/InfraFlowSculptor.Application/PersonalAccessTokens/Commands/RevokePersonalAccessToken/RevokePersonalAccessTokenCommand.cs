using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Domain.PersonalAccessTokenAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.PersonalAccessTokens.Commands.RevokePersonalAccessToken;

/// <summary>
/// Command to revoke an existing personal access token owned by the current user.
/// </summary>
/// <param name="Id">The identifier of the token to revoke.</param>
public record RevokePersonalAccessTokenCommand(PersonalAccessTokenId Id) : ICommand<bool>;
