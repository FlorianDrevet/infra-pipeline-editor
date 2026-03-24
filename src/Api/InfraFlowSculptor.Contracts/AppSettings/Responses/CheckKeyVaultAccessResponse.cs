namespace InfraFlowSculptor.Contracts.AppSettings.Responses;

/// <summary>Response DTO for the Key Vault access check.</summary>
public record CheckKeyVaultAccessResponse(
    bool HasAccess,
    string? MissingRoleDefinitionId,
    string? MissingRoleName);
