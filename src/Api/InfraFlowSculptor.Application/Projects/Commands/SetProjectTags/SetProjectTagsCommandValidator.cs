using FluentValidation;
using InfraFlowSculptor.Application.Common.Validation;

namespace InfraFlowSculptor.Application.Projects.Commands.SetProjectTags;

/// <summary>Validates <see cref="SetProjectTagsCommand"/>.</summary>
public sealed class SetProjectTagsCommandValidator : AbstractValidator<SetProjectTagsCommand>
{
    /// <summary>Initializes a new instance of the <see cref="SetProjectTagsCommandValidator"/> class.</summary>
    public SetProjectTagsCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("ProjectId is required.");

        TagValidationRules.ApplyTagRules(this, x => x.Tags);
    }
}
