using FluentValidation;

namespace InfraFlowSculptor.Application.Projects.Commands.AddProjectPipelineVariableMapping;

/// <summary>Validates the <see cref="AddProjectPipelineVariableMappingCommand"/>.</summary>
public sealed class AddProjectPipelineVariableMappingCommandValidator
    : AbstractValidator<AddProjectPipelineVariableMappingCommand>
{
    public AddProjectPipelineVariableMappingCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("ProjectId is required.");

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
