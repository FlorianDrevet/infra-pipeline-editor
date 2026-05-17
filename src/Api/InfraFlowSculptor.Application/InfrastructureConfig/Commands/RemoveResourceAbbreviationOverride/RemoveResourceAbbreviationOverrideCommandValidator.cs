using FluentValidation;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.RemoveResourceAbbreviationOverride;

/// <summary>Validates the <see cref="RemoveResourceAbbreviationOverrideCommand"/> before it is handled.</summary>
public sealed class RemoveResourceAbbreviationOverrideCommandValidator : AbstractValidator<RemoveResourceAbbreviationOverrideCommand>
{
    /// <summary>Initializes validation rules for removing a resource abbreviation override.</summary>
    public RemoveResourceAbbreviationOverrideCommandValidator()
    {
        RuleFor(x => x.InfraConfigId.Value)
            .NotEmpty().WithMessage("InfraConfigId is required.");

        RuleFor(x => x.ResourceType)
            .NotEmpty().WithMessage("ResourceType is required.")
            .MaximumLength(100).WithMessage("ResourceType must not exceed 100 characters.");
    }
}
