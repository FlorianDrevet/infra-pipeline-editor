namespace InfraFlowSculptor.Api.Configuration;

/// <summary>
/// Defines the authorization policy names and role names used by the API.
/// </summary>
public static class AuthorizationPolicyNames
{
    /// <summary>Gets the administrator policy name.</summary>
    public const string IsAdmin = nameof(IsAdmin);

    /// <summary>Gets the application role required by the administrator policy.</summary>
    public const string AdminRole = "Admin";
}
