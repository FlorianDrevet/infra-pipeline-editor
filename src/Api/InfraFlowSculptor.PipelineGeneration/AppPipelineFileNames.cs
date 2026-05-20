namespace InfraFlowSculptor.PipelineGeneration;

/// <summary>
/// Provides the repository-relative file names used by generated application pipeline wrappers.
/// </summary>
public static class AppPipelineFileNames
{
    /// <summary>Gets the generated application CI wrapper file name.</summary>
    public const string Ci = "ci.app-pipeline.yml";

    /// <summary>Gets the generated application PR validation wrapper file name.</summary>
    public const string Pr = "pr.app-pipeline.yml";

    /// <summary>Gets the generated application release wrapper file name.</summary>
    public const string Release = "release.app-pipeline.yml";
}