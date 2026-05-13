using FluentValidation;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.GeneratePipeline;

/// <summary>Validates the <see cref="GeneratePipelineCommand"/> before it is handled.</summary>
public sealed class GeneratePipelineCommandValidator : AbstractValidator<GeneratePipelineCommand>
{
    /// <summary>Initializes validation rules for generating pipeline artifacts.</summary>
    public GeneratePipelineCommandValidator()
    {
        RuleFor(x => x.InfrastructureConfigId)
            .NotEmpty().WithMessage("InfrastructureConfigId is required.");
    }
}