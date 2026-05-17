using FluentValidation;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.DownloadPipeline;

/// <summary>Validates the <see cref="DownloadPipelineCommand"/> before it is handled.</summary>
public sealed class DownloadPipelineCommandValidator : AbstractValidator<DownloadPipelineCommand>
{
    /// <summary>Initializes validation rules for downloading generated pipeline artifacts.</summary>
    public DownloadPipelineCommandValidator()
    {
        RuleFor(x => x.InfrastructureConfigId)
            .NotEmpty().WithMessage("InfrastructureConfigId is required.");
    }
}
