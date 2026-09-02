using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.DocumentIntelligences.Common;

/// <summary>Application-layer result DTO for Document Intelligence.</summary>
public record DocumentIntelligenceResult(
    AzureResourceId Id,
    ResourceGroupId ResourceGroupId,
    Name Name,
    Location Location,
    string? CustomSubDomainName,
    IReadOnlyList<DocumentIntelligenceEnvironmentConfigData> EnvironmentSettings,
    bool IsExisting = false);
