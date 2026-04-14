using InfraFlowSculptor.Domain.AppConfigurationAggregate.ValueObjects;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.Models;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Domain.AppConfigurationAggregate.Entities;

/// <summary>
/// Represents a key-value entry in an Azure App Configuration store.
/// Can be a static value, a Key Vault reference, or sourced from a pipeline variable group.
/// </summary>
public sealed class AppConfigurationKey : Entity<AppConfigurationKeyId>
{
    /// <summary>Identifier of the parent App Configuration resource.</summary>
    public AzureResourceId AppConfigurationId { get; private set; } = null!;

    /// <summary>The configuration key name (e.g., "Core:Authorization:ClientId").</summary>
    public string Key { get; private set; } = string.Empty;

    /// <summary>Optional label for the configuration key.</summary>
    public string? Label { get; private set; }

    private readonly List<AppConfigurationKeyEnvironmentValue> _environmentValues = [];

    /// <summary>
    /// Per-environment values for this static configuration key. Empty when this is a reference-based key.
    /// </summary>
    public IReadOnlyCollection<AppConfigurationKeyEnvironmentValue> EnvironmentValues => _environmentValues.AsReadOnly();

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
    /// Determines how the secret value is assigned for a Key Vault reference.
    /// Null when this is not a Key Vault reference.
    /// </summary>
    public SecretValueAssignment? SecretValueAssignment { get; private set; }

    /// <summary>
    /// Optional identifier of the pipeline variable group this key belongs to.
    /// When set, the configuration key's value comes from a pipeline variable injected at deploy time.
    /// </summary>
    public ProjectPipelineVariableGroupId? VariableGroupId { get; private set; }

    /// <summary>
    /// The name of the pipeline variable within the variable group.
    /// Required when <see cref="VariableGroupId"/> is set.
    /// </summary>
    public string? PipelineVariableName { get; private set; }

    /// <summary>Identifier of the source resource whose output is referenced. Null when not an output reference.</summary>
    public AzureResourceId? SourceResourceId { get; private set; }

    /// <summary>Name of the output on the source resource. Null when not an output reference.</summary>
    public string? SourceOutputName { get; private set; }

    private AppConfigurationKey() { }

    /// <summary>Creates a new <see cref="AppConfigurationKey"/> with per-environment static values.</summary>
    /// <param name="appConfigurationId">The owning App Configuration resource identifier.</param>
    /// <param name="key">The configuration key name.</param>
    /// <param name="label">Optional label for the key.</param>
    /// <param name="environmentValues">A dictionary of environment name → value.</param>
    internal static AppConfigurationKey CreateStatic(
        AzureResourceId appConfigurationId,
        string key,
        string? label,
        IReadOnlyDictionary<string, string> environmentValues)
    {
        var configKey = new AppConfigurationKey
        {
            Id = AppConfigurationKeyId.CreateUnique(),
            AppConfigurationId = appConfigurationId,
            Key = key,
            Label = label,
        };

        foreach (var (envName, value) in environmentValues)
        {
            configKey._environmentValues.Add(
                AppConfigurationKeyEnvironmentValue.Create(configKey.Id, envName, value));
        }

        return configKey;
    }

    /// <summary>Creates a new <see cref="AppConfigurationKey"/> referencing a Key Vault secret.</summary>
    /// <param name="appConfigurationId">The owning App Configuration resource identifier.</param>
    /// <param name="key">The configuration key name.</param>
    /// <param name="label">Optional label for the key.</param>
    /// <param name="keyVaultResourceId">The Key Vault resource identifier.</param>
    /// <param name="secretName">The secret name in the Key Vault.</param>
    /// <param name="assignment">Determines how the secret value is assigned.</param>
    internal static AppConfigurationKey CreateKeyVaultReference(
        AzureResourceId appConfigurationId,
        string key,
        string? label,
        AzureResourceId keyVaultResourceId,
        string secretName,
        SecretValueAssignment assignment)
        => new()
        {
            Id = AppConfigurationKeyId.CreateUnique(),
            AppConfigurationId = appConfigurationId,
            Key = key,
            Label = label,
            KeyVaultResourceId = keyVaultResourceId,
            SecretName = secretName,
            SecretValueAssignment = assignment,
        };

    /// <summary>Creates a new <see cref="AppConfigurationKey"/> whose value comes from a pipeline variable group at deploy time.</summary>
    /// <param name="appConfigurationId">The owning App Configuration resource identifier.</param>
    /// <param name="key">The configuration key name.</param>
    /// <param name="label">Optional label for the key.</param>
    /// <param name="variableGroupId">The pipeline variable group identifier.</param>
    /// <param name="pipelineVariableName">The pipeline variable name within the group.</param>
    internal static AppConfigurationKey CreateViaVariableGroup(
        AzureResourceId appConfigurationId,
        string key,
        string? label,
        ProjectPipelineVariableGroupId variableGroupId,
        string pipelineVariableName)
        => new()
        {
            Id = AppConfigurationKeyId.CreateUnique(),
            AppConfigurationId = appConfigurationId,
            Key = key,
            Label = label,
            VariableGroupId = variableGroupId,
            PipelineVariableName = pipelineVariableName,
        };

    /// <summary>
    /// Creates a new <see cref="AppConfigurationKey"/> whose value comes from a pipeline variable group
    /// and is stored as a Key Vault secret referenced by the configuration key.
    /// </summary>
    /// <param name="appConfigurationId">The owning App Configuration resource identifier.</param>
    /// <param name="key">The configuration key name.</param>
    /// <param name="label">Optional label for the key.</param>
    /// <param name="variableGroupId">The pipeline variable group identifier.</param>
    /// <param name="pipelineVariableName">The pipeline variable name within the group.</param>
    /// <param name="keyVaultResourceId">The Key Vault resource identifier.</param>
    /// <param name="secretName">The secret name in the Key Vault.</param>
    /// <param name="assignment">Determines how the secret value is assigned.</param>
    internal static AppConfigurationKey CreateViaVariableGroupKeyVaultReference(
        AzureResourceId appConfigurationId,
        string key,
        string? label,
        ProjectPipelineVariableGroupId variableGroupId,
        string pipelineVariableName,
        AzureResourceId keyVaultResourceId,
        string secretName,
        SecretValueAssignment assignment)
        => new()
        {
            Id = AppConfigurationKeyId.CreateUnique(),
            AppConfigurationId = appConfigurationId,
            Key = key,
            Label = label,
            VariableGroupId = variableGroupId,
            PipelineVariableName = pipelineVariableName,
            KeyVaultResourceId = keyVaultResourceId,
            SecretName = secretName,
            SecretValueAssignment = assignment,
        };

    /// <summary>Updates this configuration key to static per-environment values.</summary>
    /// <param name="key">The new configuration key name.</param>
    /// <param name="label">Optional label for the key.</param>
    /// <param name="environmentValues">A dictionary of environment name → value.</param>
    internal void UpdateToStatic(string key, string? label, IReadOnlyDictionary<string, string> environmentValues)
    {
        Key = key;
        Label = label;
        KeyVaultResourceId = null;
        SecretName = null;
        SecretValueAssignment = null;
        VariableGroupId = null;
        PipelineVariableName = null;
        SourceResourceId = null;
        SourceOutputName = null;
        _environmentValues.Clear();
        foreach (var (envName, value) in environmentValues)
        {
            _environmentValues.Add(
                AppConfigurationKeyEnvironmentValue.Create(Id, envName, value));
        }
    }

    /// <summary>Gets whether this key is a static-value key (not a reference).</summary>
    public bool IsStatic => !IsKeyVaultReference && !IsViaVariableGroup && !IsOutputReference;

    /// <summary>Gets whether this key references an output from another resource.</summary>
    public bool IsOutputReference => SourceResourceId is not null;

    /// <summary>Gets whether this key is a Key Vault secret reference.</summary>
    public bool IsKeyVaultReference => KeyVaultResourceId is not null;

    /// <summary>Gets whether this key's value comes from a pipeline variable group at deploy time.</summary>
    public bool IsViaVariableGroup => VariableGroupId is not null;

    /// <summary>Creates a new <see cref="AppConfigurationKey"/> referencing an output from a sibling resource.</summary>
    /// <param name="appConfigurationId">The owning App Configuration resource identifier.</param>
    /// <param name="key">The configuration key name.</param>
    /// <param name="label">Optional label for the key.</param>
    /// <param name="sourceResourceId">The source resource identifier.</param>
    /// <param name="sourceOutputName">The output name on the source resource.</param>
    internal static AppConfigurationKey CreateOutputReference(
        AzureResourceId appConfigurationId,
        string key,
        string? label,
        AzureResourceId sourceResourceId,
        string sourceOutputName)
        => new()
        {
            Id = AppConfigurationKeyId.CreateUnique(),
            AppConfigurationId = appConfigurationId,
            Key = key,
            Label = label,
            SourceResourceId = sourceResourceId,
            SourceOutputName = sourceOutputName,
        };

    /// <summary>Creates a new <see cref="AppConfigurationKey"/> for a sensitive output exported as a Key Vault secret.</summary>
    /// <param name="appConfigurationId">The owning App Configuration resource identifier.</param>
    /// <param name="key">The configuration key name.</param>
    /// <param name="label">Optional label for the key.</param>
    /// <param name="sourceResourceId">The source resource identifier.</param>
    /// <param name="sourceOutputName">The output name on the source resource.</param>
    /// <param name="keyVaultResourceId">The Key Vault resource identifier.</param>
    /// <param name="secretName">The secret name in the Key Vault.</param>
    internal static AppConfigurationKey CreateSensitiveOutputKeyVaultReference(
        AzureResourceId appConfigurationId,
        string key,
        string? label,
        AzureResourceId sourceResourceId,
        string sourceOutputName,
        AzureResourceId keyVaultResourceId,
        string secretName)
        => new()
        {
            Id = AppConfigurationKeyId.CreateUnique(),
            AppConfigurationId = appConfigurationId,
            Key = key,
            Label = label,
            SourceResourceId = sourceResourceId,
            SourceOutputName = sourceOutputName,
            KeyVaultResourceId = keyVaultResourceId,
            SecretName = secretName,
            SecretValueAssignment = Domain.Common.BaseModels.ValueObjects.SecretValueAssignment.DirectInKeyVault,
        };
}
