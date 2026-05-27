namespace InfraFlowSculptor.Infrastructure.Auth;

/// <summary>
/// Defines the claim names emitted for authenticated personal access token principals.
/// </summary>
public static class PersonalAccessTokenClaimNames
{
    /// <summary>Gets the claim name used to expose a granted PAT scope.</summary>
    public const string Scope = "ifs_pat_scope";
}