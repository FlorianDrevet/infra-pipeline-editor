using FluentValidation;

namespace InfraFlowSculptor.Application.AppConfigurations.Commands.RemoveAppConfigurationKey;

/// <summary>Validates the <see cref="RemoveAppConfigurationKeyCommand"/> before it is handled.</summary>
public sealed class RemoveAppConfigurationKeyCommandValidator : AbstractValidator<RemoveAppConfigurationKeyCommand>
{
    /// <summary>Initializes validation rules for removing an App Configuration key.</summary>
    public RemoveAppConfigurationKeyCommandValidator()
    {
        RuleFor(x => x.AppConfigurationId)
            .NotEmpty().WithMessage("AppConfigurationId is required.");

        RuleFor(x => x.AppConfigurationKeyId)
            .NotEmpty().WithMessage("AppConfigurationKeyId is required.");
    }
}
