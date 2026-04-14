namespace InfraFlowSculptor.Contracts.Projects.Requests;

/// <summary>Request to set the repository mode on a project.</summary>
public sealed class SetRepositoryModeRequest
{
    /// <summary>The repository mode: <c>MultiRepo</c> or <c>MonoRepo</c>.</summary>
    public required string RepositoryMode { get; init; }
}
