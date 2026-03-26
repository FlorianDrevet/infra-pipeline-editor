using FluentValidation;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.AddCrossConfigReference;

/// <summary>Validates <see cref="AddCrossConfigReferenceCommand"/>.</summary>
public sealed class AddCrossConfigReferenceCommandValidator : AbstractValidator<AddCrossConfigReferenceCommand>
{
    public AddCrossConfigReferenceCommandValidator()
    {
        RuleFor(x => x.InfraConfigId)
            .NotEmpty()
            .WithMessage("Infrastructure configuration ID is required.");

        RuleFor(x => x.TargetResourceId)
            .NotEmpty()
            .WithMessage("Target resource ID is required.");
    }
}
