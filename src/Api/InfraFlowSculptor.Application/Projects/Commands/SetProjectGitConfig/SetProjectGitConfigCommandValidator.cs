using FluentValidation;

namespace InfraFlowSculptor.Application.Projects.Commands.SetProjectGitConfig;

/// <summary>Validates the <see cref="SetProjectGitConfigCommand"/> before it is handled.</summary>
public sealed class SetProjectGitConfigCommandValidator
    : AbstractValidator<SetProjectGitConfigCommand>
{
    public SetProjectGitConfigCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("ProjectId is required.");

        RuleFor(x => x.ProviderType)
            .NotEmpty().WithMessage("ProviderType is required.")
            .Must(v => v is "GitHub" or "AzureDevOps")
            .WithMessage("ProviderType must be 'GitHub' or 'AzureDevOps'.");

        RuleFor(x => x.RepositoryUrl)
            .NotEmpty().WithMessage("RepositoryUrl is required.")
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("RepositoryUrl must be a valid absolute URL.");

        RuleFor(x => x.DefaultBranch)
            .NotEmpty().WithMessage("DefaultBranch is required.")
            .MaximumLength(200).WithMessage("DefaultBranch must not exceed 200 characters.");

        RuleFor(x => x.PersonalAccessToken)
            .NotEmpty().WithMessage("PersonalAccessToken is required.");
    }
}
