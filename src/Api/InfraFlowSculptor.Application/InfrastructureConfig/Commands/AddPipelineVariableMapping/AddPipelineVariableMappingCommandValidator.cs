using FluentValidation;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.AddPipelineVariableMapping;

/// <summary>
/// Validates the <see cref="AddPipelineVariableMappingCommand"/> before it is handled.
/// </summary>
public sealed class AddPipelineVariableMappingCommandValidator
    : AbstractValidator<AddPipelineVariableMappingCommand>
{
    public AddPipelineVariableMappingCommandValidator()
    {
        RuleFor(x => x.InfraConfigId)
            .NotEmpty().WithMessage("InfraConfigId is required.");

        RuleFor(x => x.GroupId)
            .NotEmpty().WithMessage("GroupId is required.");

        RuleFor(x => x.PipelineVariableName)
            .NotEmpty().WithMessage("PipelineVariableName is required.")
            .MaximumLength(200).WithMessage("PipelineVariableName must not exceed 200 characters.");

        RuleFor(x => x.BicepParameterName)
            .NotEmpty().WithMessage("BicepParameterName is required.")
            .MaximumLength(200).WithMessage("BicepParameterName must not exceed 200 characters.");
    }
}
