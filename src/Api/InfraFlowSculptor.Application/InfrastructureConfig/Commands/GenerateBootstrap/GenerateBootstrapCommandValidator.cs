using FluentValidation;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.GenerateBootstrap;

/// <summary>Validates the <see cref="GenerateBootstrapCommand"/> before it is handled.</summary>
public sealed class GenerateBootstrapCommandValidator : AbstractValidator<GenerateBootstrapCommand>
{
    /// <summary>Initializes validation rules for generating bootstrap artifacts.</summary>
    public GenerateBootstrapCommandValidator()
    {
        RuleFor(x => x.InfrastructureConfigId)
            .NotEmpty().WithMessage("InfrastructureConfigId is required.");
    }
}
