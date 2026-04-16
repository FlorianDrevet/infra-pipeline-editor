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

        RuleFor(x => x.Tags)
            .Must(t => t.Count <= 15).WithMessage("A maximum of 15 tags is allowed.");

        RuleForEach(x => x.Tags).ChildRules(tag =>
        {
            tag.RuleFor(t => t.Name)
                .NotEmpty().WithMessage("Tag name is required.")
                .MaximumLength(512).WithMessage("Tag name must not exceed 512 characters.");
            tag.RuleFor(t => t.Value)
                .NotEmpty().WithMessage("Tag value is required.")
                .MaximumLength(256).WithMessage("Tag value must not exceed 256 characters.");
        });
    }
}
