namespace InfraFlowSculptor.Domain.PersonalAccessTokenAggregate.ValueObjects;

/// <summary>
/// Defines the available permission scopes for personal access tokens.
/// </summary>
public enum PatScopeType
{
    /// <summary>Read-only access to projects and configurations.</summary>
    Read,

    /// <summary>Create, update, and delete projects and configurations.</summary>
    Write,

    /// <summary>Trigger Bicep and pipeline generation.</summary>
    Generate,
}
