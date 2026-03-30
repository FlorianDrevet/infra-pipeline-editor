using FluentValidation;

namespace InfraFlowSculptor.Application.Projects.Commands.RemoveProjectPipelineVariableGroup;

/// <summary>Validates the <see cref="RemoveProjectPipelineVariableGroupCommand"/>.</summary>
public sealed class RemoveProjectPipelineVariableGroupCommandValidator
    : AbstractValidator<RemoveProjectPipelineVariableGroupCommand>
{
    public RemoveProjectPipelineVariableGroupCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("ProjectId is required.");

        RuleFor(x => x.GroupId)
            .NotEmpty().WithMessage("GroupId is required.");
    }
}
