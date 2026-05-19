using FluentValidation;
using InfraFlowSculptor.Application.Common.Validation;

namespace InfraFlowSculptor.Application.Projects.Commands.AddProjectRepository;

/// <summary>Validates the <see cref="AddProjectRepositoryCommand"/> before it is handled.</summary>
public sealed class AddProjectRepositoryCommandValidator
    : AbstractValidator<AddProjectRepositoryCommand>
{
    public AddProjectRepositoryCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("ProjectId is required.");

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
