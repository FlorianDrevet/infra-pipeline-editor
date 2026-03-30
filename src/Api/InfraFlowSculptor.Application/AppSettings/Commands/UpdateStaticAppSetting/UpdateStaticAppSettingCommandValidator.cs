using FluentValidation;

namespace InfraFlowSculptor.Application.AppSettings.Commands.UpdateStaticAppSetting;

/// <summary>Validates the <see cref="UpdateStaticAppSettingCommand"/>.</summary>
public sealed class UpdateStaticAppSettingCommandValidator : AbstractValidator<UpdateStaticAppSettingCommand>
{
    /// <summary>Initializes validation rules for the update static app setting command.</summary>
    public UpdateStaticAppSettingCommandValidator()
    {
        RuleFor(x => x.ResourceId)
            .NotNull()
            .WithMessage("Resource ID is required.");

        RuleFor(x => x.AppSettingId)
            .NotNull()
            .WithMessage("App setting ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("App setting name is required.")
            .MaximumLength(256)
            .WithMessage("App setting name must not exceed 256 characters.");

        RuleFor(x => x.EnvironmentValues)
            .NotNull()
            .WithMessage("Environment values are required.")
            .Must(ev => ev is not null && ev.Count > 0)
            .WithMessage("At least one environment value must be provided.");
    }
}
