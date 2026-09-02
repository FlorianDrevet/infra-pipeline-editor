using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.DocumentIntelligences.Commands.DeleteDocumentIntelligence;

public record DeleteDocumentIntelligenceCommand(
    AzureResourceId Id
) : ICommand<Deleted>;
