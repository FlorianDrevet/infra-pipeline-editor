namespace InfraFlowSculptor.Contracts.DocumentIntelligences.Responses;

/// <summary>Response DTO for a Document Intelligence resource.</summary>
public record DocumentIntelligenceResponse(
    string Id,
    string ResourceGroupId,
    string Name,
    string Location,
    string? CustomSubDomainName,
    IReadOnlyList<DocumentIntelligenceEnvironmentConfigResponse> EnvironmentSettings,
    bool IsExisting = false);

/// <summary>Response DTO for a typed per-environment Document Intelligence configuration.</summary>
public record DocumentIntelligenceEnvironmentConfigResponse(
    string EnvironmentName,
    string? Sku,
    string? PublicNetworkAccess,
    bool DisableLocalAuth);
