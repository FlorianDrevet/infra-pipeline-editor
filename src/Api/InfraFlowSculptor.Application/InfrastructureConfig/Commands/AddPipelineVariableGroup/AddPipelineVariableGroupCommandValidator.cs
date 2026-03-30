using FluentValidation;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.AddPipelineVariableGroup;

/// <summary>
/// Validates the <see cref="AddPipelineVariableGroupCommand"/> before it is handled.
/// </summary>
public sealed class AddPipelineVariableGroupCommandValidator
    : AbstractValidator<AddPipelineVariableGroupCommand>
{
    public AddPipelineVariableGroupCommandValidator()
    {
        RuleFor(x => x.InfraConfigId)
            .NotEmpty().WithMessage("InfraConfigId is required.");

        RuleFor(x => x.GroupName)
            .NotEmpty().WithMessage("GroupName is required.")
            .MaximumLength(200).WithMessage("GroupName must not exceed 200 characters.");
    }
}
