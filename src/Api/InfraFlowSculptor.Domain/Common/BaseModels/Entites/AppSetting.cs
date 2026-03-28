using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Models;

namespace InfraFlowSculptor.Domain.Common.BaseModels.Entites;

/// <summary>
/// Represents an application setting (environment variable) configured on a compute resource.
/// Can be a static value or a reference to another resource's output.
/// </summary>
public sealed class AppSetting : Entity<AppSettingId>
{
    /// <summary>Identifier of the Azure resource that owns this setting.</summary>
    public AzureResourceId ResourceId { get; private set; } = null!;

    /// <summary>The environment variable name (e.g., KEYVAULT_URI).</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// The static value for this setting. Null when this is a resource output reference.
    /// </summary>
    public string? StaticValue { get; private set; }

    /// <summary>
    /// Identifier of the source resource whose output is referenced.
    /// Null when this is a static value.
    /// </summary>
    public AzureResourceId? SourceResourceId { get; private set; }

    /// <summary>
    /// The output name from the source resource (e.g., "vaultUri", "connectionString").
    /// Null when this is a static value.
    /// </summary>
    public string? SourceOutputName { get; private set; }

    /// <summary>
    /// Identifier of the Key Vault resource where the secret is stored.
    /// Null when this is not a Key Vault reference.
    /// </summary>
    public AzureResourceId? KeyVaultResourceId { get; private set; }

    /// <summary>
    /// The name of the secret in the Key Vault.
    /// Null when this is not a Key Vault reference.
    /// </summary>
    public string? SecretName { get; private set; }

    private AppSetting() { }

    /// <summary>Creates a new <see cref="AppSetting"/> with a static value.</summary>
    internal static AppSetting CreateStatic(
        AzureResourceId resourceId,
        string name,
        string value)
        => new()
        {
            Id = AppSettingId.CreateUnique(),
            ResourceId = resourceId,
            Name = name,
            StaticValue = value,
            SourceResourceId = null,
            SourceOutputName = null,
        };

    /// <summary>Creates a new <see cref="AppSetting"/> referencing another resource's output.</summary>
    internal static AppSetting CreateOutputReference(
        AzureResourceId resourceId,
        string name,
        AzureResourceId sourceResourceId,
        string sourceOutputName)
        => new()
        {
            Id = AppSettingId.CreateUnique(),
            ResourceId = resourceId,
            Name = name,
            StaticValue = null,
            SourceResourceId = sourceResourceId,
            SourceOutputName = sourceOutputName,
        };

    /// <summary>Creates a new <see cref="AppSetting"/> referencing a Key Vault secret.</summary>
    internal static AppSetting CreateKeyVaultReference(
        AzureResourceId resourceId,
        string name,
        AzureResourceId keyVaultResourceId,
        string secretName)
        => new()
        {
            Id = AppSettingId.CreateUnique(),
            ResourceId = resourceId,
            Name = name,
            StaticValue = null,
            SourceResourceId = null,
            SourceOutputName = null,
            KeyVaultResourceId = keyVaultResourceId,
            SecretName = secretName,
        };

    /// <summary>
    /// Creates a new <see cref="AppSetting"/> that exports a sensitive resource output
    /// as a Key Vault secret and references it via a Key Vault reference.
    /// Stores both the KV reference and the source output info for Bicep secret generation.
    /// </summary>
    internal static AppSetting CreateSensitiveOutputKeyVaultReference(
        AzureResourceId resourceId,
        string name,
        AzureResourceId sourceResourceId,
        string sourceOutputName,
        AzureResourceId keyVaultResourceId,
        string secretName)
        => new()
        {
            Id = AppSettingId.CreateUnique(),
            ResourceId = resourceId,
            Name = name,
            StaticValue = null,
            SourceResourceId = sourceResourceId,
            SourceOutputName = sourceOutputName,
            KeyVaultResourceId = keyVaultResourceId,
            SecretName = secretName,
        };

    /// <summary>Updates this app setting to a static value.</summary>
    internal void UpdateToStatic(string name, string value)
    {
        Name = name;
        StaticValue = value;
        SourceResourceId = null;
        SourceOutputName = null;
        KeyVaultResourceId = null;
        SecretName = null;
    }

    /// <summary>Updates this app setting to reference a resource output.</summary>
    internal void UpdateToOutputReference(string name, AzureResourceId sourceResourceId, string sourceOutputName)
    {
        Name = name;
        StaticValue = null;
        SourceResourceId = sourceResourceId;
        SourceOutputName = sourceOutputName;
        KeyVaultResourceId = null;
        SecretName = null;
    }

    /// <summary>Updates this app setting to reference a Key Vault secret.</summary>
    internal void UpdateToKeyVaultReference(string name, AzureResourceId keyVaultResourceId, string secretName)
    {
        Name = name;
        StaticValue = null;
        SourceResourceId = null;
        SourceOutputName = null;
        KeyVaultResourceId = keyVaultResourceId;
        SecretName = secretName;
    }

    /// <summary>Gets whether this setting is a resource output reference.</summary>
    public bool IsOutputReference => SourceResourceId is not null;

    /// <summary>Gets whether this setting is a Key Vault secret reference.</summary>
    public bool IsKeyVaultReference => KeyVaultResourceId is not null;
}
