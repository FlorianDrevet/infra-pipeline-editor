using FluentValidation;

namespace InfraFlowSculptor.Application.Projects.Commands.PushProjectMultiRepoArtifacts;

/// <summary>Validates <see cref="PushProjectMultiRepoArtifactsCommand"/> before it is handled.</summary>
public sealed class PushProjectMultiRepoArtifactsCommandValidator
    : AbstractValidator<PushProjectMultiRepoArtifactsCommand>
{
    private const int MaxBranchNameLength = 200;
    private const int MaxCommitMessageLength = 500;
    private const string BranchNamePattern = @"^[a-zA-Z0-9/_.\-]+$";

    /// <summary>Initializes the validator.</summary>
    public PushProjectMultiRepoArtifactsCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotNull().WithMessage("ProjectId is required.")
            .Must(projectId => projectId is not null && projectId.Value != Guid.Empty)
            .WithMessage("ProjectId must not be empty.");

        RuleFor(x => x.Configurations)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("At least one configuration must be provided.")
            .NotEmpty().WithMessage("At least one configuration must be provided.");

        RuleFor(x => x.Configurations)
            .Must(HasUniqueConfigurationIds)
            .WithMessage("Infrastructure configuration ids must be unique.");

        RuleForEach(x => x.Configurations)
            .ChildRules(configuration =>
            {
                configuration.RuleFor(x => x.InfrastructureConfigId)
                    .NotNull().WithMessage("InfrastructureConfigId is required.")
                    .Must(infrastructureConfigId =>
                        infrastructureConfigId is not null && infrastructureConfigId.Value != Guid.Empty)
                    .WithMessage("InfrastructureConfigId must not be empty.");

                configuration.RuleFor(x => x.Repositories)
                    .Cascade(CascadeMode.Stop)
                    .NotNull().WithMessage("At least one repository must be provided.")
                    .NotEmpty().WithMessage("At least one repository must be provided.");

                configuration.RuleFor(x => x.Repositories)
                    .Must(HasUniqueRepositoryIds)
                    .WithMessage("Repository ids must be unique within a configuration.");

                configuration.RuleForEach(x => x.Repositories)
                    .ChildRules(repository =>
                    {
                        repository.RuleFor(x => x.RepositoryId)
                            .NotNull().WithMessage("RepositoryId is required.")
                            .Must(repositoryId => repositoryId is not null && repositoryId.Value != Guid.Empty)
                            .WithMessage("RepositoryId must not be empty.");

                        repository.RuleFor(x => x.BranchName)
                            .Must(branchName => !string.IsNullOrWhiteSpace(branchName))
                            .WithMessage("BranchName is required.")
                            .MaximumLength(MaxBranchNameLength)
                            .WithMessage($"BranchName must not exceed {MaxBranchNameLength} characters.")
                            .Matches(BranchNamePattern)
                            .WithMessage("BranchName contains invalid characters.");

                        repository.RuleFor(x => x.CommitMessage)
                            .Must(commitMessage => !string.IsNullOrWhiteSpace(commitMessage))
                            .WithMessage("CommitMessage is required.")
                            .MaximumLength(MaxCommitMessageLength)
                            .WithMessage($"CommitMessage must not exceed {MaxCommitMessageLength} characters.");
                    });
            });
    }

    private static bool HasUniqueConfigurationIds(
        IReadOnlyList<InfrastructureConfigPushTarget>? configurations)
    {
        if (configurations is null)
            return false;

        var configurationIds = configurations
            .Select(configuration => configuration?.InfrastructureConfigId?.Value)
            .ToList();

        return configurationIds.Distinct().Count() == configurations.Count;
    }

    private static bool HasUniqueRepositoryIds(IReadOnlyList<ConfigRepositoryPushTarget>? repositories)
    {
        if (repositories is null)
            return false;

        var repositoryIds = repositories
            .Select(repository => repository?.RepositoryId?.Value)
            .ToList();

        return repositoryIds.Distinct().Count() == repositories.Count;
    }
}