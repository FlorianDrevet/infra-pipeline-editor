using FluentValidation;

namespace InfraFlowSculptor.Application.AppConfigurations.Commands.AddAppConfigurationKey;

/// <summary>Validates the <see cref="AddAppConfigurationKeyCommand"/>.</summary>
public sealed class AddAppConfigurationKeyCommandValidator : AbstractValidator<AddAppConfigurationKeyCommand>
{
    public AddAppConfigurationKeyCommandValidator()
    {
        RuleFor(x => x.AppConfigurationId)
            .NotNull()
            .WithMessage("App Configuration ID is required.");

        RuleFor(x => x.Key)
            .NotEmpty()
            .WithMessage("Configuration key name is required.")
            .MaximumLength(512)
            .WithMessage("Configuration key name must not exceed 512 characters.");

        RuleFor(x => x.Label)
            .MaximumLength(128)
            .WithMessage("Label must not exceed 128 characters.")
            .When(x => x.Label is not null);

        // Either environment values (static), Key Vault reference, variable group, or output reference must be provided
        RuleFor(x => x)
            .Must(x =>
                (x.EnvironmentValues is not null && x.EnvironmentValues.Count > 0)
                || (x.KeyVaultResourceId is not null && !string.IsNullOrEmpty(x.SecretName))
                || (x.VariableGroupId is not null && !string.IsNullOrEmpty(x.PipelineVariableName))
                || (x.SourceResourceId is not null && !string.IsNullOrEmpty(x.SourceOutputName)))
            .WithMessage("Either environment values (static), a Key Vault reference, a variable group reference, or an output reference must be provided.");

        RuleFor(x => x)
            .Must(x => !x.ExportToKeyVault || (x.SourceResourceId is not null && x.SourceOutputName is not null
                && x.KeyVaultResourceId is not null && !string.IsNullOrEmpty(x.SecretName)))
            .WithMessage("ExportToKeyVault requires both a source output and a Key Vault reference.")
            .When(x => x.ExportToKeyVault);

        RuleFor(x => x.SourceOutputName)
            .MaximumLength(128)
            .WithMessage("Source output name must not exceed 128 characters.")
            .When(x => x.SourceOutputName is not null);

        RuleFor(x => x.SecretName)
            .MaximumLength(256)
            .WithMessage("Secret name must not exceed 256 characters.")
            .When(x => x.SecretName is not null);

        RuleFor(x => x.SecretValueAssignment)
            .IsInEnum()
            .WithMessage("SecretValueAssignment must be a valid value (ViaBicepparam or DirectInKeyVault).")
            .When(x => x.SecretValueAssignment is not null);

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
