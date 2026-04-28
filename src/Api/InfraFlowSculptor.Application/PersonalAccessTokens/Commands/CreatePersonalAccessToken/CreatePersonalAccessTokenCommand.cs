using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.PersonalAccessTokens.Common;

namespace InfraFlowSculptor.Application.PersonalAccessTokens.Commands.CreatePersonalAccessToken;

/// <summary>
/// Command to create a new personal access token for the current authenticated user.
/// </summary>
/// <param name="Name">A human-readable label for the token.</param>
/// <param name="ExpiresAt">Optional UTC expiration date. <c>null</c> means no expiration.</param>
public record CreatePersonalAccessTokenCommand(
    string Name,
    DateTime? ExpiresAt) : ICommand<CreatedPersonalAccessTokenResult>;
