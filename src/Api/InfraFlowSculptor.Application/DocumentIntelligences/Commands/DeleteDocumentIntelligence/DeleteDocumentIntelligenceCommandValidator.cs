using FluentValidation;

namespace InfraFlowSculptor.Application.DocumentIntelligences.Commands.DeleteDocumentIntelligence;

/// <summary>Validates the <see cref="DeleteDocumentIntelligenceCommand"/>.</summary>
public sealed class DeleteDocumentIntelligenceCommandValidator : AbstractValidator<DeleteDocumentIntelligenceCommand>
{
    /// <summary>Initializes validation rules for deleting a Document Intelligence resource.</summary>
    public DeleteDocumentIntelligenceCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Id is required.");
    }
}
