using FluentValidation;

namespace InfraFlowSculptor.Application.Projects.Commands.SetProjectTags;

/// <summary>Validates <see cref="SetProjectTagsCommand"/>.</summary>
public sealed class SetProjectTagsCommandValidator : AbstractValidator<SetProjectTagsCommand>
{
    /// <summary>Initializes a new instance of the <see cref="SetProjectTagsCommandValidator"/> class.</summary>
    public SetProjectTagsCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("ProjectId is required.");

        RuleForEach(x => x.Tags).ChildRules(tag =>
        {
            tag.RuleFor(t => t.Name)
                .NotEmpty().WithMessage("Tag name is required.")
                .MaximumLength(100).WithMessage("Tag name must not exceed 100 characters.");
            tag.RuleFor(t => t.Value)
                .NotEmpty().WithMessage("Tag value is required.")
                .MaximumLength(500).WithMessage("Tag value must not exceed 500 characters.");
        });
    }
}
