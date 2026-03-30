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

    private readonly List<AppSettingEnvironmentValue> _environmentValues = [];

    /// <summary>
    /// Per-environment values for this static setting. Empty when this is a reference-based setting.
    /// </summary>
    public IReadOnlyCollection<AppSettingEnvironmentValue> EnvironmentValues => _environmentValues.AsReadOnly();

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

    /// <summary>
    /// Determines how the secret value is assigned for a static Key Vault reference.
    /// Null when this is not a static Key Vault reference.
    /// </summary>
    public SecretValueAssignment? SecretValueAssignment { get; private set; }

    private AppSetting() { }

    /// <summary>Creates a new <see cref="AppSetting"/> with per-environment static values.</summary>
    /// <param name="resourceId">The owning resource identifier.</param>
    /// <param name="name">The environment variable name.</param>
    /// <param name="environmentValues">A dictionary of environment name → value.</param>
    internal static AppSetting CreateStatic(
        AzureResourceId resourceId,
        string name,
        IReadOnlyDictionary<string, string> environmentValues)
    {
        var setting = new AppSetting
        {
            Id = AppSettingId.CreateUnique(),
            ResourceId = resourceId,
            Name = name,
            SourceResourceId = null,
            SourceOutputName = null,
        };

        foreach (var (envName, value) in environmentValues)
        {
            setting._environmentValues.Add(
                AppSettingEnvironmentValue.Create(setting.Id, envName, value));
        }

        return setting;
    }

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
            SourceResourceId = sourceResourceId,
            SourceOutputName = sourceOutputName,
        };

    /// <summary>Creates a new <see cref="AppSetting"/> referencing a Key Vault secret.</summary>
    /// <param name="resourceId">The owning resource identifier.</param>
    /// <param name="name">The environment variable name.</param>
    /// <param name="keyVaultResourceId">The Key Vault resource identifier.</param>
    /// <param name="secretName">The secret name in the Key Vault.</param>
    /// <param name="assignment">Determines how the secret value is assigned.</param>
    internal static AppSetting CreateKeyVaultReference(
        AzureResourceId resourceId,
        string name,
        AzureResourceId keyVaultResourceId,
        string secretName,
        SecretValueAssignment assignment)
        => new()
        {
            Id = AppSettingId.CreateUnique(),
            ResourceId = resourceId,
            Name = name,
            SourceResourceId = null,
            SourceOutputName = null,
            KeyVaultResourceId = keyVaultResourceId,
            SecretName = secretName,
            SecretValueAssignment = assignment,
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
            SourceResourceId = sourceResourceId,
            SourceOutputName = sourceOutputName,
            KeyVaultResourceId = keyVaultResourceId,
            SecretName = secretName,
        };

    /// <summary>Updates this app setting to static per-environment values.</summary>
    /// <param name="name">The new environment variable name.</param>
    /// <param name="environmentValues">A dictionary of environment name → value.</param>
    internal void UpdateToStatic(string name, IReadOnlyDictionary<string, string> environmentValues)
    {
        Name = name;
        SourceResourceId = null;
        SourceOutputName = null;
        KeyVaultResourceId = null;
        SecretName = null;
        _environmentValues.Clear();
        foreach (var (envName, value) in environmentValues)
        {
            _environmentValues.Add(
                AppSettingEnvironmentValue.Create(Id, envName, value));
        }
    }

    /// <summary>Updates this app setting to reference a resource output.</summary>
    internal void UpdateToOutputReference(string name, AzureResourceId sourceResourceId, string sourceOutputName)
    {
        Name = name;
        SourceResourceId = sourceResourceId;
        SourceOutputName = sourceOutputName;
        KeyVaultResourceId = null;
        SecretName = null;
        _environmentValues.Clear();
    }

    /// <summary>Updates this app setting to reference a Key Vault secret.</summary>
    /// <param name="name">The new environment variable name.</param>
    /// <param name="keyVaultResourceId">The Key Vault resource identifier.</param>
    /// <param name="secretName">The secret name in the Key Vault.</param>
    /// <param name="assignment">Determines how the secret value is assigned.</param>
    internal void UpdateToKeyVaultReference(
        string name,
        AzureResourceId keyVaultResourceId,
        string secretName,
        SecretValueAssignment assignment)
    {
        Name = name;
        SourceResourceId = null;
        SourceOutputName = null;
        KeyVaultResourceId = keyVaultResourceId;
        SecretName = secretName;
        SecretValueAssignment = assignment;
        _environmentValues.Clear();
    }   

    /// <summary>Gets whether this setting is a static-value setting (not a reference).</summary>
    public bool IsStatic => !IsOutputReference && !IsKeyVaultReference;

    /// <summary>Gets whether this setting is a static secret stored in Key Vault (not an output reference).</summary>
    public bool IsSecretStatic => IsKeyVaultReference && !IsOutputReference;

    /// <summary>Gets whether this setting is a resource output reference.</summary>
    public bool IsOutputReference => SourceResourceId is not null;

    /// <summary>Gets whether this setting is a Key Vault secret reference.</summary>
    public bool IsKeyVaultReference => KeyVaultResourceId is not null;
}
