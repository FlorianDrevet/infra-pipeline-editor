using FluentValidation;

namespace InfraFlowSculptor.Application.Projects.Commands.SetAgentPool;

/// <summary>Validates the <see cref="SetAgentPoolCommand"/> before it is handled.</summary>
public sealed class SetAgentPoolCommandValidator : AbstractValidator<SetAgentPoolCommand>
{
    /// <summary>Initializes the validator.</summary>
    public SetAgentPoolCommandValidator()
    {
        RuleFor(x => x.ProjectId.Value)
            .NotEmpty().WithMessage("ProjectId is required.");

        When(
            x => !string.IsNullOrWhiteSpace(x.AgentPoolName),
            () =>
            {
                RuleFor(x => x.AgentPoolName!)
                    .MaximumLength(200).WithMessage("AgentPoolName must not exceed 200 characters.");
            });
    }
}