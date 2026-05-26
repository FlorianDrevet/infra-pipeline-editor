using FluentValidation;

namespace InfraFlowSculptor.Application.Git.Commands.VerifyGitConnection;

/// <summary>Validates the <see cref="VerifyGitConnectionCommand"/> before it is handled.</summary>
public sealed class VerifyGitConnectionCommandValidator : AbstractValidator<VerifyGitConnectionCommand>
{
    private static readonly string[] AllowedProviderTypes = ["GitHub", "AzureDevOps", "GitLab", "Bitbucket"];

    /// <summary>Initializes validation rules for <see cref="VerifyGitConnectionCommand"/>.</summary>
    public VerifyGitConnectionCommandValidator()
    {
        RuleFor(x => x.ProviderType)
            .NotEmpty().WithMessage("ProviderType is required.")
            .Must(value => AllowedProviderTypes.Contains(value, StringComparer.OrdinalIgnoreCase))
            .WithMessage("ProviderType must be one of: GitHub, AzureDevOps, GitLab, Bitbucket.");

        RuleFor(x => x.RepositoryUrl)
            .NotEmpty().WithMessage("RepositoryUrl is required.")
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("RepositoryUrl must be a valid absolute URL.");

        RuleFor(x => x.PersonalAccessToken)
            .NotEmpty().WithMessage("PersonalAccessToken is required.");
    }
}
