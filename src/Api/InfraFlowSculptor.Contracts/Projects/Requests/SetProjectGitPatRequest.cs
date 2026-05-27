namespace InfraFlowSculptor.Contracts.Projects.Requests;

/// <summary>
/// Request body for storing or updating the project-scoped Git personal access token.
/// </summary>
public class SetProjectGitPatRequest
{
    /// <summary>
    /// The personal access token stored in Key Vault and shared across the project's repositories.
    /// </summary>
    public required string PersonalAccessToken { get; init; }
}