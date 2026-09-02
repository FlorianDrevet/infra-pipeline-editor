using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.DocumentIntelligences.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;

namespace InfraFlowSculptor.Application.DocumentIntelligences.Commands.UpdateDocumentIntelligence;

public record UpdateDocumentIntelligenceCommand(
    AzureResourceId Id,
    Name Name,
    Location Location,
    string? CustomSubDomainName,
    IReadOnlyList<DocumentIntelligenceEnvironmentConfigData>? EnvironmentSettings = null
) : ICommand<DocumentIntelligenceResult>;
