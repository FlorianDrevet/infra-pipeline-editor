namespace InfraFlowSculptor.Infrastructure.Services.BlobService;

/// <summary>
/// Configures the blob container used to store generated artifacts.
/// </summary>
public sealed class BlobSettings
{
    /// <summary>The configuration section name for blob storage settings.</summary>
    public const string SectionName = "BlobSettings";

    /// <summary>The default container name used when configuration does not provide one.</summary>
    public const string DefaultContainerName = "bicep-output";

    /// <summary>Gets the blob container name used for generated artifacts.</summary>
    public string ContainerName { get; init; } = DefaultContainerName;

    /// <summary>
    /// Resolves the blob container name, falling back to the repository default when unset.
    /// </summary>
    /// <param name="containerName">The configured container name.</param>
    /// <returns>The configured container name or the default container name.</returns>
    public static string ResolveContainerName(string? containerName)
    {
        return string.IsNullOrWhiteSpace(containerName)
            ? DefaultContainerName
            : containerName.Trim();
    }
}