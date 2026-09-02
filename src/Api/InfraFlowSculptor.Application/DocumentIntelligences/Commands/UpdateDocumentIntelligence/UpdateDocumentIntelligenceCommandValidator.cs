using FluentValidation;

namespace InfraFlowSculptor.Application.DocumentIntelligences.Commands.UpdateDocumentIntelligence;

/// <summary>
/// Validates the <see cref="UpdateDocumentIntelligenceCommand"/> before it is handled.
/// </summary>
public sealed class UpdateDocumentIntelligenceCommandValidator : AbstractValidator<UpdateDocumentIntelligenceCommand>
{
    /// <summary>Initializes validation rules for updating a Document Intelligence resource.</summary>
    public UpdateDocumentIntelligenceCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.");
    }
}
