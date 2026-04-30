using FluentValidation;

namespace InfraFlowSculptor.Application.Projects.Commands.CreateProjectWithSetup;

/// <summary>Validates the <see cref="CreateProjectWithSetupCommand"/> before it is handled.</summary>
public sealed class CreateProjectWithSetupCommandValidator
    : AbstractValidator<CreateProjectWithSetupCommand>
{
    private static readonly string[] AllowedLayouts =
        ["AllInOne", "SplitInfraCode", "MultiRepo"];

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "Tracked under test-debt #22: refactoring deferred until dedicated unit-test coverage protects against behavioural regressions. The method orchestrates a single coherent business operation and would lose readability without proper test guards.")]
    public CreateProjectWithSetupCommandValidator()
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

        // Layout / repositories cross-rule.
        RuleFor(x => x).Custom((cmd, ctx) =>
        {
            var repos = cmd.Repositories ?? [];
            switch (cmd.LayoutPreset)
            {
                case "MultiRepo":
                    if (repos.Count > 0)
                        ctx.AddFailure("Repositories",
                            "MultiRepo layout must not declare project-level repositories.");
                    break;

                case "AllInOne":
                    if (repos.Count != 1)
                        ctx.AddFailure("Repositories",
                            "AllInOne layout requires exactly one repository.");
                    break;

                case "SplitInfraCode":
                    if (repos.Count != 2)
                        ctx.AddFailure("Repositories",
                            "SplitInfraCode layout requires exactly two repositories (Infrastructure + ApplicationCode).");
                    break;
            }
        });

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
                .Must(v => v is null or "GitHub" or "AzureDevOps")
                .WithMessage("ProviderType must be 'GitHub' or 'AzureDevOps'.");
            repo.RuleFor(r => r).Custom((r, ctx) =>
            {
                var hasUrl = !string.IsNullOrWhiteSpace(r.RepositoryUrl);
                var hasBranch = !string.IsNullOrWhiteSpace(r.DefaultBranch);
                var hasProvider = !string.IsNullOrWhiteSpace(r.ProviderType);
                var anySet = hasUrl || hasBranch || hasProvider;
                var allSet = hasUrl && hasBranch && hasProvider;
                if (anySet && !allSet)
                {
                    ctx.AddFailure("ConnectionDetails",
                        "ProviderType, RepositoryUrl and DefaultBranch must be either all provided or all empty.");
                }
            });
        });
    }
}
