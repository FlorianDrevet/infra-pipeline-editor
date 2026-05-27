using FluentValidation;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.CreateInfraConfig;

/// <summary>Validates the <see cref="CreateInfrastructureConfigCommand"/>.</summary>
public sealed class CreateInfrastructureConfigCommandValidator : AbstractValidator<CreateInfrastructureConfigCommand>
{
    /// <summary>Initializes a new instance of the <see cref="CreateInfrastructureConfigCommandValidator"/> class.</summary>
    public CreateInfrastructureConfigCommandValidator()
    {
        RuleFor(command => command.Name)
            .NotEmpty().WithMessage("Infrastructure configuration name is required.")
            .MaximumLength(100).WithMessage("Infrastructure configuration name must not exceed 100 characters.");
    }
}
