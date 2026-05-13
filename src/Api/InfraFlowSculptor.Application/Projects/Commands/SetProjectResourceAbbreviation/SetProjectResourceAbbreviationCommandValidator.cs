using FluentValidation;
using InfraFlowSculptor.GenerationCore;

namespace InfraFlowSculptor.Application.Projects.Commands.SetProjectResourceAbbreviation;

/// <summary>Validates <see cref="SetProjectResourceAbbreviationCommand"/>.</summary>
public sealed class SetProjectResourceAbbreviationCommandValidator
    : AbstractValidator<SetProjectResourceAbbreviationCommand>
{
    /// <inheritdoc />
    public SetProjectResourceAbbreviationCommandValidator()
    {
        RuleFor(x => x.ResourceType)
            .NotEmpty()
            .MaximumLength(100)
            .Must(resourceType => AzureResourceTypes.All.Contains(resourceType))
            .WithMessage("ResourceType must be a supported Azure resource type.");

        RuleFor(x => x.Abbreviation)
            .NotEmpty()
            .MaximumLength(10)
            .Matches("^[a-z0-9]+$")
            .WithMessage("Abbreviation must contain only lowercase alphanumeric characters.");
    }
}
