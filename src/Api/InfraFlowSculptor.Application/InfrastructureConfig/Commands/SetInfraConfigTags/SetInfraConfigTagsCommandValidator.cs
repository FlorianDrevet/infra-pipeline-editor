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
