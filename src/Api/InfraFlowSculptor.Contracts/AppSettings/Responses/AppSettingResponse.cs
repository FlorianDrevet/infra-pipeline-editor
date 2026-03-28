namespace InfraFlowSculptor.Contracts.AppSettings.Responses;

/// <summary>Response DTO for an app setting.</summary>
public record AppSettingResponse(
    string Id,
    string ResourceId,
    string Name,
    Dictionary<string, string>? EnvironmentValues,
    string? SourceResourceId,
    string? SourceOutputName,
    bool IsOutputReference,
    string? KeyVaultResourceId,
    string? SecretName,
    bool IsKeyVaultReference,
    bool? HasKeyVaultAccess,
    string? SecretValueAssignment);
