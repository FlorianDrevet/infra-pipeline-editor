namespace InfraFlowSculptor.GenerationCore;

/// <summary>
/// Builds Azure DevOps pipeline definition names that must stay aligned across
/// bootstrap provisioning and downstream YAML pipeline references.
/// </summary>
public static class AzureDevOpsPipelineNameHelper
{
    /// <summary>Gets the display prefix used for infrastructure-owned pipelines.</summary>
    public const string InfrastructurePrefix = "[Infra]";

    /// <summary>Gets the display prefix used for application-code-owned pipelines.</summary>
    public const string CodePrefix = "[Code]";

    private const string SegmentSeparator = " - ";
    private const string CiSuffix = "CI";
    private const string PrSuffix = "PR";
    private const string ReleaseSuffix = "Release";

    /// <summary>
    /// Builds the infrastructure CI pipeline definition name for a configuration.
    /// </summary>
    /// <param name="configName">The infrastructure configuration name.</param>
    /// <returns>The prefixed Azure DevOps pipeline definition name.</returns>
    public static string BuildInfrastructureCiName(string configName)
    {
        return BuildName(InfrastructurePrefix, PathSanitizer.Sanitize(configName), CiSuffix);
    }

    /// <summary>
    /// Builds the infrastructure PR pipeline definition name for a configuration.
    /// </summary>
    /// <param name="configName">The infrastructure configuration name.</param>
    /// <returns>The prefixed Azure DevOps pipeline definition name.</returns>
    public static string BuildInfrastructurePrName(string configName)
    {
        return BuildName(InfrastructurePrefix, PathSanitizer.Sanitize(configName), PrSuffix);
    }

    /// <summary>
    /// Builds the infrastructure release pipeline definition name for a configuration.
    /// </summary>
    /// <param name="configName">The infrastructure configuration name.</param>
    /// <returns>The prefixed Azure DevOps pipeline definition name.</returns>
    public static string BuildInfrastructureReleaseName(string configName)
    {
        return BuildName(InfrastructurePrefix, PathSanitizer.Sanitize(configName), ReleaseSuffix);
    }

    /// <summary>
    /// Builds the application CI pipeline definition name for a configuration/resource pair.
    /// </summary>
    /// <param name="configName">The infrastructure configuration name.</param>
    /// <param name="resourceName">The application resource name.</param>
    /// <returns>The prefixed Azure DevOps pipeline definition name.</returns>
    public static string BuildApplicationCiName(string configName, string resourceName)
    {
        return BuildName(CodePrefix, PathSanitizer.Sanitize(configName), PathSanitizer.Sanitize(resourceName), CiSuffix);
    }

    /// <summary>
    /// Builds the application release pipeline definition name for a configuration/resource pair.
    /// </summary>
    /// <param name="configName">The infrastructure configuration name.</param>
    /// <param name="resourceName">The application resource name.</param>
    /// <returns>The prefixed Azure DevOps pipeline definition name.</returns>
    public static string BuildApplicationReleaseName(string configName, string resourceName)
    {
        return BuildName(CodePrefix, PathSanitizer.Sanitize(configName), PathSanitizer.Sanitize(resourceName), ReleaseSuffix);
    }

    private static string BuildName(string prefix, params string[] segments)
    {
        return $"{prefix} {string.Join(SegmentSeparator, segments)}";
    }
}