using FluentValidation;
using InfraFlowSculptor.Application.Common.Validation;

namespace InfraFlowSculptor.Application.Projects.Commands.AddProjectRepository;

/// <summary>Validates the <see cref="AddProjectRepositoryCommand"/> before it is handled.</summary>
public sealed class AddProjectRepositoryCommandValidator
    : AbstractValidator<AddProjectRepositoryCommand>
{
    private const string AliasPattern = "^[a-z0-9-]+$";

    public AddProjectRepositoryCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("ProjectId is required.");

        RuleFor(x => x.Alias)
            .NotEmpty().WithMessage("Alias is required.")
            .MaximumLength(50).WithMessage("Alias must not exceed 50 characters.")
            .Matches(AliasPattern).WithMessage("Alias must contain only lowercase letters, digits and hyphens.");

        // Connection details are optional, but when provided they must be valid and consistent.
        RepositoryConnectionValidationRules.Apply(this,
            cmd => cmd.ProviderType,
            x => x.RepositoryUrl,
            x => x.DefaultBranch);

        RuleFor(x => x.ContentKinds)
            .NotNull().WithMessage("ContentKinds is required.")
            .Must(c => c is { Count: > 0 }).WithMessage("At least one content kind must be provided.");
    }
}
