using FluentValidation;

namespace InfraFlowSculptor.Application.AppSettings.Commands.AddAppSetting;

/// <summary>Validates the <see cref="AddAppSettingCommand"/>.</summary>
public sealed class AddAppSettingCommandValidator : AbstractValidator<AddAppSettingCommand>
{
    public AddAppSettingCommandValidator()
    {
        RuleFor(x => x.ResourceId)
            .NotNull()
            .WithMessage("Resource ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("App setting name is required.")
            .MaximumLength(256)
            .WithMessage("App setting name must not exceed 256 characters.");

        // Either static value or output reference must be provided
        RuleFor(x => x)
            .Must(x => !string.IsNullOrEmpty(x.StaticValue) || (x.SourceResourceId is not null && !string.IsNullOrEmpty(x.SourceOutputName)))
            .WithMessage("Either a static value or a source resource output reference must be provided.");
    }
}
