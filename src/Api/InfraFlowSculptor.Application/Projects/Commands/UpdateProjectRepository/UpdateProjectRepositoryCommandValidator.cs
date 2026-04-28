using FluentValidation;
using InfraFlowSculptor.Application.Common.Validation;

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

        RepositoryConnectionValidationRules.Apply(this,
            cmd => cmd.ProviderType,
            x => x.RepositoryUrl,
            x => x.DefaultBranch);

        RuleFor(x => x.ContentKinds)
            .NotNull().WithMessage("ContentKinds is required.")
            .Must(c => c is { Count: > 0 }).WithMessage("At least one content kind must be provided.");
    }
}
