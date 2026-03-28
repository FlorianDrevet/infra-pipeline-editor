using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.AppSettings.Common;

/// <summary>Application-layer result for an app setting.</summary>
public sealed record AppSettingResult(
    AppSettingId Id,
    AzureResourceId ResourceId,
    string Name,
    IReadOnlyDictionary<string, string>? EnvironmentValues,
    AzureResourceId? SourceResourceId,
    string? SourceOutputName,
    bool IsOutputReference,
    AzureResourceId? KeyVaultResourceId,
    string? SecretName,
    bool IsKeyVaultReference,
    bool? HasKeyVaultAccess,
    SecretValueAssignment? SecretValueAssignment);
