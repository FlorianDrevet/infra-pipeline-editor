using FluentValidation;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.GenerateAppPipeline;

/// <summary>Validates the <see cref="GenerateAppPipelineCommand"/> before it is handled.</summary>
public sealed class GenerateAppPipelineCommandValidator : AbstractValidator<GenerateAppPipelineCommand>
{
    /// <summary>Initializes validation rules for the generate app pipeline command.</summary>
    public GenerateAppPipelineCommandValidator()
    {
        RuleFor(x => x.InfrastructureConfigId)
            .NotEmpty().WithMessage("InfrastructureConfigId is required.");

        RuleFor(x => x.ResourceId)
            .NotEmpty().WithMessage("ResourceId is required.");
    }
}
