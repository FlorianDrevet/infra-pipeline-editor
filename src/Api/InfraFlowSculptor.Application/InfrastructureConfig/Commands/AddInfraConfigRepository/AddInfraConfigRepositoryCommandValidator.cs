using FluentValidation;
using InfraFlowSculptor.Application.Common.Validation;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.AddInfraConfigRepository;

/// <summary>Validates the <see cref="AddInfraConfigRepositoryCommand"/> before it is handled.</summary>
public sealed class AddInfraConfigRepositoryCommandValidator
    : AbstractValidator<AddInfraConfigRepositoryCommand>
{
    private const string AliasPattern = "^[a-z0-9-]+$";

    /// <summary>Initializes the validator.</summary>
    public AddInfraConfigRepositoryCommandValidator()
    {
        RuleFor(x => x.ProjectId.Value)
            .NotEmpty().WithMessage("ProjectId is required.");

        RuleFor(x => x.ConfigId.Value)
            .NotEmpty().WithMessage("ConfigId is required.");

        RuleFor(x => x.Alias)
            .NotEmpty().WithMessage("Alias is required.")
            .MaximumLength(50).WithMessage("Alias must not exceed 50 characters.")
            .Matches(AliasPattern).WithMessage("Alias must contain only lowercase letters, digits and hyphens.");

        RuleFor(x => x.ProviderType)
            .NotEmpty().WithMessage("ProviderType is required.");

        RuleFor(x => x.RepositoryUrl)
            .NotEmpty().WithMessage("RepositoryUrl is required.");

        RuleFor(x => x.DefaultBranch)
            .NotEmpty().WithMessage("DefaultBranch is required.");

        RepositoryConnectionValidationRules.Apply(
            this,
            x => x.ProviderType,
            x => x.RepositoryUrl,
            x => x.DefaultBranch);

        RuleFor(x => x.ContentKinds)
            .NotNull().WithMessage("ContentKinds is required.")
            .Must(contentKinds => contentKinds is { Count: > 0 })
            .WithMessage("At least one content kind must be provided.");
    }
}
