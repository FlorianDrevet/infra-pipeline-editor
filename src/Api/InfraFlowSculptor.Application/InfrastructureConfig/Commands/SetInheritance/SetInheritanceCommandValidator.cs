using FluentValidation;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.SetInheritance;

/// <summary>Validates the <see cref="SetInheritanceCommand"/> before it is handled.</summary>
public sealed class SetInheritanceCommandValidator : AbstractValidator<SetInheritanceCommand>
{
    /// <summary>Initializes validation rules for toggling project inheritance.</summary>
    public SetInheritanceCommandValidator()
    {
        RuleFor(x => x.InfraConfigId.Value)
            .NotEmpty().WithMessage("InfraConfigId is required.");
    }
}