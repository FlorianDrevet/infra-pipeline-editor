using FluentValidation;

namespace InfraFlowSculptor.Application.Projects.Commands.CreateProjectWithSetup;

/// <summary>Validates the <see cref="CreateProjectWithSetupCommand"/> before it is handled.</summary>
public sealed class CreateProjectWithSetupCommandValidator
    : AbstractValidator<CreateProjectWithSetupCommand>
{
    private const string LayoutAllInOne = "AllInOne";
    private const string LayoutSplitInfraCode = "SplitInfraCode";
    private const string LayoutMultiRepo = "MultiRepo";

    private static readonly string[] AllowedLayouts =
        [LayoutAllInOne, LayoutSplitInfraCode, LayoutMultiRepo];

    private static readonly string[] AllowedProviderTypes =
        ["GitHub", "AzureDevOps"];

    public CreateProjectWithSetupCommandValidator()
    {
        ConfigureCoreFields();
        ConfigureEnvironments();
        ConfigureLayoutRepositoriesRule();
        ConfigureRepositoryRules();
    }

    private void ConfigureCoreFields()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Project name is required.")
            .MinimumLength(3).WithMessage("Project name must be at least 3 characters.")
            .MaximumLength(80).WithMessage("Project name must not exceed 80 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).When(x => x.Description is not null);

        RuleFor(x => x.LayoutPreset)
            .NotEmpty().WithMessage("Layout preset is required.")
            .Must(v => AllowedLayouts.Contains(v))
            .WithMessage("LayoutPreset must be 'AllInOne', 'SplitInfraCode' or 'MultiRepo'.");
    }

    private void ConfigureEnvironments()
    {
        RuleFor(x => x.Environments)
            .NotNull()
            .Must(e => e is { Count: > 0 })
            .WithMessage("At least one environment must be defined.");

        RuleForEach(x => x.Environments).ChildRules(env =>
        {
            env.RuleFor(e => e.Name).NotEmpty().MaximumLength(100);
            env.RuleFor(e => e.ShortName).NotEmpty().MaximumLength(20);
            env.RuleFor(e => e.Location).NotEmpty();
            env.RuleFor(e => e.Order).GreaterThanOrEqualTo(0);
        });
    }

    private void ConfigureLayoutRepositoriesRule()
    {
        RuleFor(x => x).Custom((cmd, ctx) =>
        {
            var repos = cmd.Repositories ?? [];
            var error = ValidateLayoutRepositories(cmd.LayoutPreset, repos.Count);
            if (error is not null)
                ctx.AddFailure("Repositories", error);
        });
    }

    private static string? ValidateLayoutRepositories(string layoutPreset, int repoCount) => layoutPreset switch
    {
        LayoutMultiRepo when repoCount > 0 =>
            "MultiRepo layout must not declare project-level repositories.",
        LayoutAllInOne when repoCount != 1 =>
            "AllInOne layout requires exactly one repository.",
        LayoutSplitInfraCode when repoCount != 2 =>
            "SplitInfraCode layout requires exactly two repositories (Infrastructure + ApplicationCode).",
        _ => null,
    };

    private void ConfigureRepositoryRules()
    {
        RuleForEach(x => x.Repositories).ChildRules(repo =>
        {
            repo.RuleFor(r => r.Alias)
                .NotEmpty().MaximumLength(50)
                .Matches("^[a-z0-9-]+$")
                .WithMessage("Alias must contain only lowercase letters, digits and hyphens.");
            repo.RuleFor(r => r.ContentKinds)
                .NotNull().Must(c => c is { Count: > 0 })
                .WithMessage("At least one content kind is required per repository.");
            repo.RuleFor(r => r.ProviderType)
                .Must(v => v is null || AllowedProviderTypes.Contains(v))
                .WithMessage("ProviderType must be 'GitHub' or 'AzureDevOps'.");
            repo.RuleFor(r => r).Custom((r, ctx) =>
            {
                if (HasIncompleteConnectionDetails(r.RepositoryUrl, r.DefaultBranch, r.ProviderType))
                {
                    ctx.AddFailure("ConnectionDetails",
                        "ProviderType, RepositoryUrl and DefaultBranch must be either all provided or all empty.");
                }
            });
        });
    }

    private static bool HasIncompleteConnectionDetails(string? repositoryUrl, string? defaultBranch, string? providerType)
    {
        var hasUrl = !string.IsNullOrWhiteSpace(repositoryUrl);
        var hasBranch = !string.IsNullOrWhiteSpace(defaultBranch);
        var hasProvider = !string.IsNullOrWhiteSpace(providerType);
        var anySet = hasUrl || hasBranch || hasProvider;
        var allSet = hasUrl && hasBranch && hasProvider;
        return anySet && !allSet;
    }
}
