using FluentValidation;

namespace InfraFlowSculptor.Application.Projects.Commands.GenerateProjectBootstrapPipeline;

/// <summary>Validates the <see cref="GenerateProjectBootstrapPipelineCommand"/> before it is handled.</summary>
public sealed class GenerateProjectBootstrapPipelineCommandValidator : AbstractValidator<GenerateProjectBootstrapPipelineCommand>
{
    /// <summary>Initializes validation rules for generating a project bootstrap pipeline.</summary>
    public GenerateProjectBootstrapPipelineCommandValidator()
    {
        RuleFor(x => x.ProjectId.Value)
            .NotEmpty().WithMessage("ProjectId is required.");
    }
}
