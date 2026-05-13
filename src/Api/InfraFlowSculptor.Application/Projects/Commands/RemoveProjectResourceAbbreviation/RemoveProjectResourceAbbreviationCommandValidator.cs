using FluentValidation;

namespace InfraFlowSculptor.Application.Projects.Commands.RemoveProjectResourceAbbreviation;

/// <summary>Validates the <see cref="RemoveProjectResourceAbbreviationCommand"/> before it is handled.</summary>
public sealed class RemoveProjectResourceAbbreviationCommandValidator : AbstractValidator<RemoveProjectResourceAbbreviationCommand>
{
    /// <summary>Initializes validation rules for removing a project resource abbreviation.</summary>
    public RemoveProjectResourceAbbreviationCommandValidator()
    {
        RuleFor(x => x.ProjectId.Value)
            .NotEmpty().WithMessage("ProjectId is required.");

        RuleFor(x => x.ResourceType)
            .NotEmpty().WithMessage("ResourceType is required.")
            .MaximumLength(100).WithMessage("ResourceType must not exceed 100 characters.");
    }
}