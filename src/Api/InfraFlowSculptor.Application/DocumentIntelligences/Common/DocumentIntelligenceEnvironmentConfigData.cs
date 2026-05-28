namespace InfraFlowSculptor.Application.DocumentIntelligences.Common;

/// <summary>Per-environment configuration data for Document Intelligence resources.</summary>
public record DocumentIntelligenceEnvironmentConfigData(
    string EnvironmentName,
    string? Sku,
    string? PublicNetworkAccess,
    bool DisableLocalAuth);
