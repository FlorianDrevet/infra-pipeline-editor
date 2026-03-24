using FluentValidation;

namespace InfraFlowSculptor.Application.AppConfigurations.Commands.CreateAppConfiguration;

/// <summary>
/// Validates the <see cref="CreateAppConfigurationCommand"/> before it is handled.
/// </summary>
public sealed class CreateAppConfigurationCommandValidator : AbstractValidator<CreateAppConfigurationCommand>
{
    /// <summary>Initializes validation rules for creating an App Configuration.</summary>
    public CreateAppConfigurationCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.");

        RuleFor(x => x.ResourceGroupId)
            .NotEmpty().WithMessage("ResourceGroupId is required.");
    }
}
