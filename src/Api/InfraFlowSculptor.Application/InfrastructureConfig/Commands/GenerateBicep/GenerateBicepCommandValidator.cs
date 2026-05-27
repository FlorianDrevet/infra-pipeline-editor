using FluentValidation;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.GenerateBicep;

/// <summary>Validates the <see cref="GenerateBicepCommand"/> before it is handled.</summary>
public sealed class GenerateBicepCommandValidator : AbstractValidator<GenerateBicepCommand>
{
    /// <summary>Initializes validation rules for generating Bicep artifacts.</summary>
    public GenerateBicepCommandValidator()
    {
        RuleFor(x => x.InfrastructureConfigId)
            .NotEmpty().WithMessage("InfrastructureConfigId is required.");
    }
}
