using FluentValidation;

namespace InfraFlowSculptor.Application.Projects.Commands.UpdateProjectRepository;

/// <summary>Validates the <see cref="UpdateProjectRepositoryCommand"/> before it is handled.</summary>
public sealed class UpdateProjectRepositoryCommandValidator
    : AbstractValidator<UpdateProjectRepositoryCommand>
{
    public UpdateProjectRepositoryCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("ProjectId is required.");

        RuleFor(x => x.RepositoryId)
            .NotEmpty().WithMessage("RepositoryId is required.");

        RuleFor(x => x.ProviderType)
            .Must(v => v is null or "GitHub" or "AzureDevOps")
            .WithMessage("ProviderType must be 'GitHub' or 'AzureDevOps'.");

        When(x => !string.IsNullOrWhiteSpace(x.RepositoryUrl), () =>
        {
            RuleFor(x => x.RepositoryUrl!)
                .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
                .WithMessage("RepositoryUrl must be a valid absolute URL.");
        });

        RuleFor(x => x.DefaultBranch)
            .MaximumLength(200).WithMessage("DefaultBranch must not exceed 200 characters.");

        RuleFor(x => x).Custom((cmd, ctx) =>
        {
            var hasUrl = !string.IsNullOrWhiteSpace(cmd.RepositoryUrl);
            var hasBranch = !string.IsNullOrWhiteSpace(cmd.DefaultBranch);
            var hasProvider = !string.IsNullOrWhiteSpace(cmd.ProviderType);
            var anySet = hasUrl || hasBranch || hasProvider;
            var allSet = hasUrl && hasBranch && hasProvider;
            if (anySet && !allSet)
            {
                ctx.AddFailure(
                    "ConnectionDetails",
                    "ProviderType, RepositoryUrl and DefaultBranch must be either all provided or all empty.");
            }
        });

        RuleFor(x => x.ContentKinds)
            .NotNull().WithMessage("ContentKinds is required.")
            .Must(c => c is { Count: > 0 }).WithMessage("At least one content kind must be provided.");
    }
}
