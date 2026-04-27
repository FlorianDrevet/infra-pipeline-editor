using FluentValidation;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.SetResourceAbbreviationOverride;

/// <summary>Validates <see cref="SetResourceAbbreviationOverrideCommand"/>.</summary>
public sealed class SetResourceAbbreviationOverrideCommandValidator
    : AbstractValidator<SetResourceAbbreviationOverrideCommand>
{
    /// <inheritdoc />
    public SetResourceAbbreviationOverrideCommandValidator()
    {
        RuleFor(x => x.ResourceType).NotEmpty().MaximumLength(100);

        RuleFor(x => x.Abbreviation)
            .NotEmpty()
            .MaximumLength(10)
            .Matches("^[a-z0-9]+$")
            .WithMessage("Abbreviation must contain only lowercase alphanumeric characters.");
    }
}
