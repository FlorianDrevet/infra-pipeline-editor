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

        // Either environment values (static), output reference, Key Vault reference, or variable group must be provided
        RuleFor(x => x)
            .Must(x =>
                (x.EnvironmentValues is not null && x.EnvironmentValues.Count > 0)
                || (x.SourceResourceId is not null && !string.IsNullOrEmpty(x.SourceOutputName))
                || (x.KeyVaultResourceId is not null && !string.IsNullOrEmpty(x.SecretName))
                || (x.VariableGroupId is not null && !string.IsNullOrEmpty(x.PipelineVariableName)))
            .WithMessage("Either environment values (static), a source resource output reference, a Key Vault reference, or a variable group reference must be provided.");

        // ExportToKeyVault requires both source output AND Key Vault fields
        RuleFor(x => x)
            .Must(x =>
                x.SourceResourceId is not null
                && !string.IsNullOrEmpty(x.SourceOutputName)
                && x.KeyVaultResourceId is not null
                && !string.IsNullOrEmpty(x.SecretName))
            .When(x => x.ExportToKeyVault)
            .WithMessage("ExportToKeyVault requires both source output reference and Key Vault reference fields.");

        RuleFor(x => x.SecretName)
            .MaximumLength(256)
            .WithMessage("Secret name must not exceed 256 characters.")
            .When(x => x.SecretName is not null);

        // SecretValueAssignment must be a valid enum value when provided
        RuleFor(x => x.SecretValueAssignment)
            .IsInEnum()
            .WithMessage("SecretValueAssignment must be a valid value (ViaBicepparam or DirectInKeyVault).")
            .When(x => x.SecretValueAssignment is not null);

        // VariableGroupId requires PipelineVariableName
        RuleFor(x => x.PipelineVariableName)
            .NotEmpty()
            .WithMessage("PipelineVariableName is required when VariableGroupId is set.")
            .When(x => x.VariableGroupId is not null);

        RuleFor(x => x.PipelineVariableName)
            .MaximumLength(256)
            .WithMessage("PipelineVariableName must not exceed 256 characters.")
            .When(x => x.PipelineVariableName is not null);
    }
}
