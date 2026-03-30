using FluentValidation;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.RemovePipelineVariableGroup;

/// <summary>
/// Validates the <see cref="RemovePipelineVariableGroupCommand"/> before it is handled.
/// </summary>
public sealed class RemovePipelineVariableGroupCommandValidator
    : AbstractValidator<RemovePipelineVariableGroupCommand>
{
    public RemovePipelineVariableGroupCommandValidator()
    {
        RuleFor(x => x.InfraConfigId)
            .NotEmpty().WithMessage("InfraConfigId is required.");

        RuleFor(x => x.GroupId)
            .NotEmpty().WithMessage("GroupId is required.");
    }
}
