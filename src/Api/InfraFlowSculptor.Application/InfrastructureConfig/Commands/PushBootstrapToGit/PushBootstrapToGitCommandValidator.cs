using FluentValidation;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.PushBootstrapToGit;

/// <summary>Validates the <see cref="PushBootstrapToGitCommand"/>.</summary>
public sealed class PushBootstrapToGitCommandValidator : AbstractValidator<PushBootstrapToGitCommand>
{
    /// <summary>Initializes validation rules for pushing generated bootstrap artifacts to Git.</summary>
    public PushBootstrapToGitCommandValidator()
    {
        RuleFor(x => x.InfrastructureConfigId).NotEmpty();
        RuleFor(x => x.BranchName).NotEmpty().MaximumLength(250);
        RuleFor(x => x.CommitMessage).NotEmpty().MaximumLength(500);
    }
}
