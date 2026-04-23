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

        RuleFor(x => x.ContentKinds)
            .NotNull().WithMessage("ContentKinds is required.")
            .Must(c => c is { Count: > 0 }).WithMessage("At least one content kind must be provided.");
    }
}
