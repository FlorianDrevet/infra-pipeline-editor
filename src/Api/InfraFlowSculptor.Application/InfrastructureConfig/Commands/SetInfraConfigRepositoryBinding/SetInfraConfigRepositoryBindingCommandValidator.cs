using FluentValidation;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.SetInfraConfigRepositoryBinding;

/// <summary>Validates the <see cref="SetInfraConfigRepositoryBindingCommand"/>.</summary>
public sealed class SetInfraConfigRepositoryBindingCommandValidator
    : AbstractValidator<SetInfraConfigRepositoryBindingCommand>
{
    /// <summary>Creates a new validator instance.</summary>
    public SetInfraConfigRepositoryBindingCommandValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.ConfigId).NotEmpty();

        When(x => !string.IsNullOrWhiteSpace(x.RepositoryAlias), () =>
        {
            RuleFor(x => x.RepositoryAlias!)
                .Matches("^[a-z0-9-]+$")
                .WithMessage("RepositoryAlias must be lowercase alphanumeric or hyphen.")
                .MaximumLength(50);
        });

        When(x => x.Branch is not null, () =>
        {
            RuleFor(x => x.Branch!)
                .NotEmpty()
                .WithMessage("Branch must not be blank when provided.")
                .MaximumLength(200);
        });

        When(x => x.InfraPath is not null, () =>
        {
            RuleFor(x => x.InfraPath!).MaximumLength(500);
        });

        When(x => x.PipelinePath is not null, () =>
        {
            RuleFor(x => x.PipelinePath!).MaximumLength(500);
        });
    }
}
