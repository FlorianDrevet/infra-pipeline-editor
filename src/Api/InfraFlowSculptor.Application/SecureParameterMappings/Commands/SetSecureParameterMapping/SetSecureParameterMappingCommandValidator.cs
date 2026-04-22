using FluentValidation;

namespace InfraFlowSculptor.Application.SecureParameterMappings.Commands.SetSecureParameterMapping;

/// <summary>Validates the <see cref="SetSecureParameterMappingCommand"/> before it is handled.</summary>
public sealed class SetSecureParameterMappingCommandValidator : AbstractValidator<SetSecureParameterMappingCommand>
{
    private const string PipelineVariableNamePattern = @"^[a-zA-Z_][a-zA-Z0-9_]*$";

    /// <summary>Initializes a new instance of the <see cref="SetSecureParameterMappingCommandValidator"/> class.</summary>
    public SetSecureParameterMappingCommandValidator()
    {
        RuleFor(x => x.SecureParameterName)
            .NotEmpty()
            .WithMessage("SecureParameterName is required.");

        RuleFor(x => x.PipelineVariableName)
            .NotEmpty()
            .WithMessage("PipelineVariableName is required when VariableGroupId is set.")
            .When(x => x.VariableGroupId is not null);

        RuleFor(x => x.PipelineVariableName)
            .Matches(PipelineVariableNamePattern)
            .WithMessage("PipelineVariableName must start with a letter or underscore and contain only letters, digits, and underscores.")
            .When(x => x.VariableGroupId is not null && !string.IsNullOrWhiteSpace(x.PipelineVariableName));

        RuleFor(x => x.PipelineVariableName)
            .Null()
            .WithMessage("PipelineVariableName must be null when VariableGroupId is null.")
            .When(x => x.VariableGroupId is null);
    }
}
