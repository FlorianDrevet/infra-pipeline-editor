namespace InfraFlowSculptor.Infrastructure.Auth;

/// <summary>
/// Default constants for the personal access token authentication scheme.
/// </summary>
public static class PersonalAccessTokenAuthenticationDefaults
{
    /// <summary>The default authentication scheme name.</summary>
    public const string AuthenticationScheme = "PersonalAccessToken";

    /// <summary>The prefix that every valid personal access token starts with.</summary>
    public const string TokenPrefix = "ifs_";
}
