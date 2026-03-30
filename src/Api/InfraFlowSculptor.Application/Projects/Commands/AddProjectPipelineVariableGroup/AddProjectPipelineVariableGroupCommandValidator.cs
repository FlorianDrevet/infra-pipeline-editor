using FluentValidation;

namespace InfraFlowSculptor.Application.Projects.Commands.AddProjectPipelineVariableGroup;

/// <summary>Validates the <see cref="AddProjectPipelineVariableGroupCommand"/>.</summary>
public sealed class AddProjectPipelineVariableGroupCommandValidator
    : AbstractValidator<AddProjectPipelineVariableGroupCommand>
{
    public AddProjectPipelineVariableGroupCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("ProjectId is required.");

        RuleFor(x => x.GroupName)
            .NotEmpty().WithMessage("GroupName is required.")
            .MaximumLength(200).WithMessage("GroupName must not exceed 200 characters.");
    }
}
