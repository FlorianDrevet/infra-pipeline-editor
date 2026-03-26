namespace InfraFlowSculptor.BicepGeneration.Models;

/// <summary>
/// A secondary Bicep module file generated alongside a primary resource module.
/// Used for child resource deployments (e.g. blob service + containers alongside a Storage Account).
/// </summary>
public sealed record GeneratedCompanionModule
{
    /// <summary>Suffix used to build the Bicep module symbol (e.g. <c>Blobs</c>).</summary>
    public string ModuleSymbolSuffix { get; init; } = string.Empty;

    /// <summary>Suffix used to build the deployment name (e.g. <c>Blobs</c>).</summary>
    public string DeploymentNameSuffix { get; init; } = string.Empty;

    /// <summary>File name of the companion module (e.g. "storage.blobs.module.bicep").</summary>
    public string FileName { get; init; } = string.Empty;

    /// <summary>Folder under <c>modules/</c> where the file is placed (e.g. "StorageAccount").</summary>
    public string FolderName { get; init; } = string.Empty;

    /// <summary>Bicep content of the companion module.</summary>
    public string BicepContent { get; init; } = string.Empty;

    /// <summary>Optional types.bicep content that must be merged into the folder's types file.</summary>
    public string TypesBicepContent { get; init; } = string.Empty;

    /// <summary>Blob container names to provision (static configuration, not per-env).</summary>
    public IReadOnlyList<string> BlobContainerNames { get; init; } = [];

    /// <summary>Storage table names to provision (static configuration, not per-env).</summary>
    public IReadOnlyList<string> StorageTableNames { get; init; } = [];

    /// <summary>CORS rules to apply to the blob service.</summary>
    public IReadOnlyList<BlobCorsRuleData> CorsRules { get; init; } = [];
}
