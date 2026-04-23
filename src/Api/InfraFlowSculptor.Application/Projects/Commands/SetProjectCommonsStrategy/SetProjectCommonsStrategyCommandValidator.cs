using FluentValidation;

namespace InfraFlowSculptor.Application.Projects.Commands.SetProjectCommonsStrategy;

/// <summary>Validates the <see cref="SetProjectCommonsStrategyCommand"/>.</summary>
public sealed class SetProjectCommonsStrategyCommandValidator
    : AbstractValidator<SetProjectCommonsStrategyCommand>
{
    /// <summary>Creates a new validator instance.</summary>
    public SetProjectCommonsStrategyCommandValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.Strategy)
            .NotEmpty()
            .WithMessage("Strategy is required.");
    }
}
