using FluentValidation;

namespace InfraFlowSculptor.Application.NetworkingProfiles.Commands.ToggleResourcePrivatization;

/// <summary>Validates <see cref="ToggleResourcePrivatizationCommand"/> inputs.</summary>
public sealed class ToggleResourcePrivatizationCommandValidator : AbstractValidator<ToggleResourcePrivatizationCommand>
{
    public ToggleResourcePrivatizationCommandValidator()
    {
        RuleFor(x => x.InfraConfigId).NotNull();
        RuleFor(x => x.ResourceId).NotNull();
    }
}
