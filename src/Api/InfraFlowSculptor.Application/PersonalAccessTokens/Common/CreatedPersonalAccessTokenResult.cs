namespace InfraFlowSculptor.Application.PersonalAccessTokens.Common;

/// <summary>
/// Result returned after creating a personal access token.
/// Extends <see cref="PersonalAccessTokenResult"/> with the one-time plaintext token value.
/// </summary>
/// <param name="PlainTextToken">The full plaintext token — displayed only once at creation.</param>
public record CreatedPersonalAccessTokenResult(
    PersonalAccessTokenResult Token,
    string PlainTextToken);
