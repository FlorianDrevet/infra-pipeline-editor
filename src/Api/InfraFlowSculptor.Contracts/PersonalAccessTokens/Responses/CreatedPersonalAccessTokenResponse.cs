namespace InfraFlowSculptor.Contracts.PersonalAccessTokens.Responses;

/// <summary>
/// Response returned once when creating a new personal access token.
/// Contains the one-time plaintext token value that must be copied immediately.
/// </summary>
/// <param name="Token">The token metadata.</param>
/// <param name="PlainTextToken">The full plaintext token — displayed only once at creation.</param>
public record CreatedPersonalAccessTokenResponse(
    PersonalAccessTokenResponse Token,
    string PlainTextToken);
