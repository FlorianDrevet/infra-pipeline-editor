using FluentValidation;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.SetInfraConfigTags;

/// <summary>Validates <see cref="SetInfraConfigTagsCommand"/>.</summary>
public sealed class SetInfraConfigTagsCommandValidator : AbstractValidator<SetInfraConfigTagsCommand>
{
    /// <summary>Initializes a new instance of the <see cref="SetInfraConfigTagsCommandValidator"/> class.</summary>
    public SetInfraConfigTagsCommandValidator()
    {
        RuleFor(x => x.InfraConfigId)
            .NotEmpty().WithMessage("InfraConfigId is required.");

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
