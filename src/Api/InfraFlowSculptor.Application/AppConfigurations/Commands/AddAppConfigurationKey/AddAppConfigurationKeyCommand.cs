using InfraFlowSculptor.Application.AppConfigurations.Common;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.AppConfigurations.Commands.AddAppConfigurationKey;

/// <summary>Command to add a configuration key to an App Configuration resource.</summary>
/// <param name="AppConfigurationId">Identifier of the App Configuration resource.</param>
/// <param name="Key">The configuration key name.</param>
/// <param name="Label">Optional label for the key.</param>
/// <param name="EnvironmentValues">Per-environment values for a static key. Null when using a reference.</param>
/// <param name="SourceResourceId">Identifier of the source resource whose output is referenced.</param>
/// <param name="SourceOutputName">Name of the output on the source resource.</param>
/// <param name="ExportToKeyVault">When true, a sensitive output is exported as a Key Vault secret.</param>
/// <param name="KeyVaultResourceId">Identifier of the Key Vault resource for a KV reference.</param>
/// <param name="SecretName">The secret name in the Key Vault.</param>
/// <param name="SecretValueAssignment">Determines how the secret value is assigned for a Key Vault reference.</param>
/// <param name="VariableGroupId">Optional identifier of the pipeline variable group.</param>
/// <param name="PipelineVariableName">The pipeline variable name within the variable group.</param>
public record AddAppConfigurationKeyCommand(
    AzureResourceId AppConfigurationId,
    string Key,
    string? Label,
    IReadOnlyDictionary<string, string>? EnvironmentValues,
    AzureResourceId? SourceResourceId = null,
    string? SourceOutputName = null,
    bool ExportToKeyVault = false,
    AzureResourceId? KeyVaultResourceId = null,
    string? SecretName = null,
    SecretValueAssignment? SecretValueAssignment = null,
    Guid? VariableGroupId = null,
    string? PipelineVariableName = null
) : ICommand<AppConfigurationKeyResult>;
