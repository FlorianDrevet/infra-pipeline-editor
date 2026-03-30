using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Application.AppSettings.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.AppSettings.Commands.AddAppSetting;

/// <summary>Command to add an app setting (environment variable) to a compute resource.</summary>
/// <param name="ResourceId">Identifier of the compute resource (WebApp, FunctionApp, or ContainerApp).</param>
/// <param name="Name">The environment variable name.</param>
/// <param name="StaticValue">The static value (null when using a resource output reference).</param>
/// <param name="SourceResourceId">Identifier of the source resource for output reference (null for static values).</param>
/// <param name="SourceOutputName">The output name on the source resource (null for static values).</param>
/// <param name="KeyVaultResourceId">Identifier of the Key Vault resource for a KV reference (null for non-KV settings).</param>
/// <param name="SecretName">The secret name in the Key Vault (null for non-KV settings).</param>
/// <param name="ExportToKeyVault">When true, a sensitive output is exported as a Key Vault secret and referenced via KV reference.</param>
/// <param name="SecretValueAssignment">Determines how the secret value is assigned for a static Key Vault reference (null defaults to DirectInKeyVault).</param>
public record AddAppSettingCommand(
    AzureResourceId ResourceId,
    string Name,
    IReadOnlyDictionary<string, string>? EnvironmentValues,
    AzureResourceId? SourceResourceId,
    string? SourceOutputName,
    AzureResourceId? KeyVaultResourceId,
    string? SecretName,
    bool ExportToKeyVault = false,
    SecretValueAssignment? SecretValueAssignment = null
) : ICommand<AppSettingResult>;
