namespace InfraFlowSculptor.BicepGeneration.Models;

/// <summary>
/// Represents an app setting (environment variable) to be injected into a compute resource module.
/// Can be a static value, a reference to another resource's output, or a Key Vault secret reference.
/// </summary>
public sealed class AppSettingDefinition
{
    /// <summary>The environment variable name (e.g. "KeyVaultUri").</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Static value (when <see cref="IsOutputReference"/> is <c>false</c>).</summary>
    public string? StaticValue { get; init; }

    /// <summary>Logical name of the source resource that exposes the output.</summary>
    public string? SourceResourceName { get; init; }

    /// <summary>Output name on the source resource (e.g. "vaultUri").</summary>
    public string? SourceOutputName { get; init; }

    /// <summary>Azure resource type name of the source (e.g. "KeyVault").</summary>
    public string? SourceResourceTypeName { get; init; }

    /// <summary>Logical name of the target resource this setting belongs to.</summary>
    public string TargetResourceName { get; init; } = string.Empty;

    /// <summary>Whether this setting references another module's output.</summary>
    public bool IsOutputReference { get; init; }

    /// <summary>
    /// The Bicep expression for the output (e.g. "kv.properties.vaultUri").
    /// Resolved from the <c>ResourceOutputCatalog</c> by the handler.
    /// </summary>
    public string? SourceOutputBicepExpression { get; init; }

    /// <summary>Whether this setting is a Key Vault secret reference.</summary>
    public bool IsKeyVaultReference { get; init; }

    /// <summary>Logical name of the Key Vault resource (used to build the module reference).</summary>
    public string? KeyVaultResourceName { get; init; }

    /// <summary>The name of the secret in the Key Vault.</summary>
    public string? SecretName { get; init; }
}
