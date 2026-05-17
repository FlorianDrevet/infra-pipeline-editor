using FluentValidation;

namespace InfraFlowSculptor.Application.AppSettings.Commands.RemoveAppSetting;

/// <summary>Validates the <see cref="RemoveAppSettingCommand"/> before it is handled.</summary>
public sealed class RemoveAppSettingCommandValidator : AbstractValidator<RemoveAppSettingCommand>
{
    /// <summary>Initializes validation rules for removing an app setting.</summary>
    public RemoveAppSettingCommandValidator()
    {
        RuleFor(x => x.ResourceId)
            .NotEmpty().WithMessage("ResourceId is required.");

        RuleFor(x => x.AppSettingId)
            .NotEmpty().WithMessage("AppSettingId is required.");
    }
}
