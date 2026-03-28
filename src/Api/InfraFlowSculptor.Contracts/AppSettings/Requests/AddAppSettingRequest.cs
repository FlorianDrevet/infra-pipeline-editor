using System.ComponentModel.DataAnnotations;

namespace InfraFlowSculptor.Contracts.AppSettings.Requests;

/// <summary>Request to add an app setting to a compute resource.</summary>
public class AddAppSettingRequest
{
    /// <summary>The environment variable name (e.g., KEYVAULT_URI).</summary>
    [Required]
    [MaxLength(256)]
    public required string Name { get; init; }

    /// <summary>Per-environment values for a static setting. Null when using a reference.</summary>
    public Dictionary<string, string>? EnvironmentValues { get; init; }

    /// <summary>Identifier of the source resource for output reference (null for static values).</summary>
    public Guid? SourceResourceId { get; init; }

    /// <summary>The output name on the source resource (null for static values).</summary>
    [MaxLength(128)]
    public string? SourceOutputName { get; init; }

    /// <summary>Identifier of the Key Vault resource for a KV reference (null for non-KV settings).</summary>
    public Guid? KeyVaultResourceId { get; init; }

    /// <summary>The secret name in the Key Vault (null for non-KV settings).</summary>
    [MaxLength(256)]
    public string? SecretName { get; init; }

    /// <summary>
    /// When <c>true</c>, a sensitive resource output will be exported as a Key Vault secret
    /// and a Key Vault reference will be used for the app setting instead of a direct value.
    /// Requires <see cref="KeyVaultResourceId"/> and <see cref="SourceResourceId"/>/<see cref="SourceOutputName"/> to be set.
    /// </summary>
    public bool ExportToKeyVault { get; init; }
}
