using FluentValidation;

namespace InfraFlowSculptor.Application.Projects.Commands.SetProjectLayoutPreset;

/// <summary>Validates the <see cref="SetProjectLayoutPresetCommand"/>.</summary>
public sealed class SetProjectLayoutPresetCommandValidator
    : AbstractValidator<SetProjectLayoutPresetCommand>
{
    /// <summary>Creates a new validator instance.</summary>
    public SetProjectLayoutPresetCommandValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.Preset)
            .NotEmpty()
            .WithMessage("Preset is required.");
    }
}
