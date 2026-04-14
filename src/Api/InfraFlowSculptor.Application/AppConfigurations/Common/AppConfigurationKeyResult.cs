using InfraFlowSculptor.Domain.AppConfigurationAggregate.ValueObjects;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.AppConfigurations.Common;

/// <summary>Application-layer result for an App Configuration key.</summary>
public sealed record AppConfigurationKeyResult(
    AppConfigurationKeyId Id,
    AzureResourceId AppConfigurationId,
    string Key,
    string? Label,
    IReadOnlyDictionary<string, string>? EnvironmentValues,
    AzureResourceId? SourceResourceId,
    string? SourceOutputName,
    bool IsOutputReference,
    AzureResourceId? KeyVaultResourceId,
    string? SecretName,
    bool IsKeyVaultReference,
    bool? HasKeyVaultAccess,
    SecretValueAssignment? SecretValueAssignment,
    Guid? VariableGroupId,
    string? PipelineVariableName,
    string? VariableGroupName,
    bool IsViaVariableGroup);
