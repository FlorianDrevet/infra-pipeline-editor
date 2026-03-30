using FluentValidation;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.RemovePipelineVariableMapping;

/// <summary>
/// Validates the <see cref="RemovePipelineVariableMappingCommand"/> before it is handled.
/// </summary>
public sealed class RemovePipelineVariableMappingCommandValidator
    : AbstractValidator<RemovePipelineVariableMappingCommand>
{
    public RemovePipelineVariableMappingCommandValidator()
    {
        RuleFor(x => x.InfraConfigId)
            .NotEmpty().WithMessage("InfraConfigId is required.");

        RuleFor(x => x.GroupId)
            .NotEmpty().WithMessage("GroupId is required.");

        RuleFor(x => x.MappingId)
            .NotEmpty().WithMessage("MappingId is required.");
    }
}
