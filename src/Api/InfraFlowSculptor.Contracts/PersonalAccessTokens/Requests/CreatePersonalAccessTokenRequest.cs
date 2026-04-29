namespace InfraFlowSculptor.Contracts.PersonalAccessTokens.Requests;

/// <summary>
/// Request body for creating a new personal access token.
/// </summary>
/// <param name="Name">A human-readable label for the token.</param>
/// <param name="ExpiresAt">Optional UTC expiration date. <c>null</c> means no expiration.</param>
public record CreatePersonalAccessTokenRequest(
    string Name,
    DateTime? ExpiresAt);
