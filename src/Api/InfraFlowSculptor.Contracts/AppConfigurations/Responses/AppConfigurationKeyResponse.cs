namespace InfraFlowSculptor.Contracts.AppConfigurations.Responses;

/// <summary>Response DTO for an App Configuration key.</summary>
public record AppConfigurationKeyResponse(
    string Id,
    string AppConfigurationId,
    string Key,
    string? Label,
    Dictionary<string, string>? EnvironmentValues,
    string? KeyVaultResourceId,
    string? SecretName,
    bool IsKeyVaultReference,
    bool? HasKeyVaultAccess,
    string? SecretValueAssignment,
    string? VariableGroupId,
    string? PipelineVariableName,
    string? VariableGroupName,
    bool IsViaVariableGroup);
