using FluentValidation;

namespace InfraFlowSculptor.Application.Common.Validation;

/// <summary>
/// Shared repository connection validation rules applied by both
/// <c>AddProjectRepositoryCommandValidator</c> and <c>UpdateProjectRepositoryCommandValidator</c>.
/// </summary>
internal static class RepositoryConnectionValidationRules
{
    /// <summary>
    /// Applies the standard repository connection rules: provider type must be null/GitHub/AzureDevOps,
    /// URL must be absolute when set, branch max length, and all-or-nothing consistency.
    /// </summary>
    /// <typeparam name="T">The command type being validated.</typeparam>
    /// <param name="validator">The validator to add rules to.</param>
    /// <param name="providerTypeSelector">Function to extract the provider type from the command.</param>
    /// <param name="urlSelector">Function to extract the repository URL from the command.</param>
    /// <param name="branchSelector">Function to extract the default branch from the command.</param>
    internal static void Apply<T>(
        AbstractValidator<T> validator,
        Func<T, string?> providerTypeSelector,
        System.Linq.Expressions.Expression<Func<T, string?>> urlSelector,
        System.Linq.Expressions.Expression<Func<T, string?>> branchSelector)
    {
        validator.RuleFor(x => x)
            .Must(cmd => providerTypeSelector(cmd) is null or "GitHub" or "AzureDevOps")
            .WithMessage("ProviderType must be 'GitHub' or 'AzureDevOps'.");

        validator.When(x => !string.IsNullOrWhiteSpace(urlSelector.Compile()(x)), () =>
        {
            validator.RuleFor(urlSelector!)
                .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
                .WithMessage("RepositoryUrl must be a valid absolute URL.");
        });

        validator.RuleFor(branchSelector)
            .MaximumLength(200).WithMessage("DefaultBranch must not exceed 200 characters.");

        validator.RuleFor(x => x).Custom((cmd, ctx) =>
        {
            var hasUrl = !string.IsNullOrWhiteSpace(urlSelector.Compile()(cmd));
            var hasBranch = !string.IsNullOrWhiteSpace(branchSelector.Compile()(cmd));
            var hasProvider = !string.IsNullOrWhiteSpace(providerTypeSelector(cmd));
            var anySet = hasUrl || hasBranch || hasProvider;
            var allSet = hasUrl && hasBranch && hasProvider;
            if (anySet && !allSet)
            {
                ctx.AddFailure(
                    "ConnectionDetails",
                    "ProviderType, RepositoryUrl and DefaultBranch must be either all provided or all empty.");
            }
        });
    }
}
