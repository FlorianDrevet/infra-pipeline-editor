using System.ComponentModel.DataAnnotations;

namespace InfraFlowSculptor.Contracts.AppConfigurations.Requests;

/// <summary>Request to add a configuration key to an App Configuration resource.</summary>
public class AddAppConfigurationKeyRequest
{
    /// <summary>The configuration key name (e.g., "Core:Authorization:ClientId").</summary>
    [Required]
    [MaxLength(512)]
    public required string Key { get; init; }

    /// <summary>Optional label for the configuration key.</summary>
    [MaxLength(128)]
    public string? Label { get; init; }

    /// <summary>Per-environment values for a static key. Null when using a reference.</summary>
    public Dictionary<string, string>? EnvironmentValues { get; init; }

    /// <summary>Identifier of the Key Vault resource for a KV reference (null for non-KV keys).</summary>
    public Guid? KeyVaultResourceId { get; init; }

    /// <summary>The secret name in the Key Vault (null for non-KV keys).</summary>
    [MaxLength(256)]
    public string? SecretName { get; init; }

    /// <summary>
    /// Determines how the secret value is assigned for a Key Vault reference.
    /// Valid values: "ViaBicepparam" or "DirectInKeyVault". Defaults to "DirectInKeyVault" if not provided.
    /// </summary>
    public string? SecretValueAssignment { get; init; }

    /// <summary>
    /// Optional identifier of the pipeline variable group this key belongs to.
    /// When set, the configuration key's value comes from a pipeline variable injected at deploy time.
    /// </summary>
    public Guid? VariableGroupId { get; init; }

    /// <summary>
    /// The name of the pipeline variable within the variable group.
    /// Required when <see cref="VariableGroupId"/> is set.
    /// </summary>
    [MaxLength(256)]
    public string? PipelineVariableName { get; init; }
}
