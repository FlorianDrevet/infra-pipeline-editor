using FluentValidation;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.PushPipelineToGit;

/// <summary>Validates the <see cref="PushPipelineToGitCommand"/>.</summary>
public sealed class PushPipelineToGitCommandValidator : AbstractValidator<PushPipelineToGitCommand>
{
    public PushPipelineToGitCommandValidator()
    {
        RuleFor(x => x.InfrastructureConfigId).NotEmpty();
        RuleFor(x => x.BranchName).NotEmpty().MaximumLength(250);
        RuleFor(x => x.CommitMessage).NotEmpty().MaximumLength(500);
    }
}
