using FluentValidation;

namespace InfraFlowSculptor.Application.AppConfigurations.Commands.UpdateAppConfiguration;

/// <summary>
/// Validates the <see cref="UpdateAppConfigurationCommand"/> before it is handled.
/// </summary>
public sealed class UpdateAppConfigurationCommandValidator : AbstractValidator<UpdateAppConfigurationCommand>
{
    /// <summary>Initializes validation rules for updating an App Configuration resource.</summary>
    public UpdateAppConfigurationCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.");
    }
}