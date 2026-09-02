using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.DocumentIntelligences.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.DocumentIntelligences.Queries;

public record GetDocumentIntelligenceQuery(
    AzureResourceId Id
) : IQuery<DocumentIntelligenceResult>;
