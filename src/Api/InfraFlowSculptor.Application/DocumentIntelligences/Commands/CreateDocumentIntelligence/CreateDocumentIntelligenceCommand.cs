using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.DocumentIntelligences.Common;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.DocumentIntelligences.Commands.CreateDocumentIntelligence;

public record CreateDocumentIntelligenceCommand(
    ResourceGroupId ResourceGroupId,
    Name Name,
    Location Location,
    string? CustomSubDomainName,
    IReadOnlyList<DocumentIntelligenceEnvironmentConfigData>? EnvironmentSettings = null,
    bool IsExisting = false
) : ICommand<DocumentIntelligenceResult>, ICreateResourceCommand;
