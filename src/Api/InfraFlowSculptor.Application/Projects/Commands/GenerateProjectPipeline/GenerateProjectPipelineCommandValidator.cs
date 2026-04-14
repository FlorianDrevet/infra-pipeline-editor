using FluentValidation;

namespace InfraFlowSculptor.Application.Projects.Commands.GenerateProjectPipeline;

/// <summary>Validates the <see cref="GenerateProjectPipelineCommand"/> before it is handled.</summary>
public sealed class GenerateProjectPipelineCommandValidator
    : AbstractValidator<GenerateProjectPipelineCommand>
{
    public GenerateProjectPipelineCommandValidator()
    {
        RuleFor(x => x.ProjectId.Value)
            .NotEmpty().WithMessage("ProjectId is required.");
    }
}
