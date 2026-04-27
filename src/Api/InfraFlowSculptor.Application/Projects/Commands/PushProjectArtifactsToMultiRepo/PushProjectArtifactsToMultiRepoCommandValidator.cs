using FluentValidation;

namespace InfraFlowSculptor.Application.Projects.Commands.PushProjectArtifactsToMultiRepo;

/// <summary>Validates <see cref="PushProjectArtifactsToMultiRepoCommand"/> before it is handled.</summary>
public sealed class PushProjectArtifactsToMultiRepoCommandValidator : AbstractValidator<PushProjectArtifactsToMultiRepoCommand>
{
    /// <summary>Initializes the validator.</summary>
    public PushProjectArtifactsToMultiRepoCommandValidator()
    {
        RuleFor(x => x.ProjectId).NotNull().WithMessage("ProjectId is required.");

        RuleFor(x => x)
            .Must(x => x.Infra != null || x.Code != null)
            .WithMessage("At least one push target must be provided.");

        When(x => x.Infra != null, () =>
        {
            RuleFor(x => x.Infra!.Alias)
                .NotEmpty().WithMessage("Infra alias is required.")
                .MaximumLength(255).WithMessage("Infra alias must not exceed 255 characters.");
            RuleFor(x => x.Infra!.BranchName)
                .NotEmpty().WithMessage("Infra branch name is required.")
                .MaximumLength(255).WithMessage("Infra branch name must not exceed 255 characters.");
            RuleFor(x => x.Infra!.CommitMessage)
                .NotEmpty().WithMessage("Infra commit message is required.")
                .MaximumLength(2000).WithMessage("Infra commit message must not exceed 2000 characters.");
        });

        When(x => x.Code != null, () =>
        {
            RuleFor(x => x.Code!.Alias)
                .NotEmpty().WithMessage("Code alias is required.")
                .MaximumLength(255).WithMessage("Code alias must not exceed 255 characters.");
            RuleFor(x => x.Code!.BranchName)
                .NotEmpty().WithMessage("Code branch name is required.")
                .MaximumLength(255).WithMessage("Code branch name must not exceed 255 characters.");
            RuleFor(x => x.Code!.CommitMessage)
                .NotEmpty().WithMessage("Code commit message is required.")
                .MaximumLength(2000).WithMessage("Code commit message must not exceed 2000 characters.");
        });
    }
}
