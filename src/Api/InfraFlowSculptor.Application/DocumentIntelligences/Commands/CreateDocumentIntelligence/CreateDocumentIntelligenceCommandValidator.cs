using InfraFlowSculptor.Application.Common.Validation;

namespace InfraFlowSculptor.Application.DocumentIntelligences.Commands.CreateDocumentIntelligence;

/// <summary>
/// Validates the <see cref="CreateDocumentIntelligenceCommand"/> before it is handled.
/// </summary>
public sealed class CreateDocumentIntelligenceCommandValidator : CreateResourceCommandValidator<CreateDocumentIntelligenceCommand>
{
}
