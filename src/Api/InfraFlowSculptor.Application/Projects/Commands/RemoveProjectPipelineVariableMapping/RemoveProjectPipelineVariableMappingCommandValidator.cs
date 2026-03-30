using FluentValidation;

namespace InfraFlowSculptor.Application.Projects.Commands.RemoveProjectPipelineVariableMapping;

/// <summary>Validates the <see cref="RemoveProjectPipelineVariableMappingCommand"/>.</summary>
public sealed class RemoveProjectPipelineVariableMappingCommandValidator
    : AbstractValidator<RemoveProjectPipelineVariableMappingCommand>
{
    public RemoveProjectPipelineVariableMappingCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("ProjectId is required.");

        RuleFor(x => x.GroupId)
            .NotEmpty().WithMessage("GroupId is required.");

        RuleFor(x => x.MappingId)
            .NotEmpty().WithMessage("MappingId is required.");
    }
}
