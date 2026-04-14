using FluentValidation;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.RemoveCrossConfigReference;

/// <summary>Validates <see cref="RemoveCrossConfigReferenceCommand"/>.</summary>
public sealed class RemoveCrossConfigReferenceCommandValidator : AbstractValidator<RemoveCrossConfigReferenceCommand>
{
    public RemoveCrossConfigReferenceCommandValidator()
    {
        RuleFor(x => x.InfraConfigId)
            .NotEmpty()
            .WithMessage("Infrastructure configuration ID is required.");

        RuleFor(x => x.ReferenceId)
            .NotEmpty()
            .WithMessage("Reference ID is required.");
    }
}
