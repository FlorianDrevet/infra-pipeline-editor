using FluentValidation;
using InfraFlowSculptor.Application.Common.Validation;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.SetInfraConfigTags;

/// <summary>Validates <see cref="SetInfraConfigTagsCommand"/>.</summary>
public sealed class SetInfraConfigTagsCommandValidator : AbstractValidator<SetInfraConfigTagsCommand>
{
    /// <summary>Initializes a new instance of the <see cref="SetInfraConfigTagsCommandValidator"/> class.</summary>
    public SetInfraConfigTagsCommandValidator()
    {
        RuleFor(x => x.InfraConfigId)
            .NotEmpty().WithMessage("InfraConfigId is required.");

        TagValidationRules.ApplyTagRules(this, x => x.Tags);
    }
}
