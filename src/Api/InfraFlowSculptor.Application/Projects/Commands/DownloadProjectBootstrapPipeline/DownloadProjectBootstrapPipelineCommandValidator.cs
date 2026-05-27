using FluentValidation;

namespace InfraFlowSculptor.Application.Projects.Commands.DownloadProjectBootstrapPipeline;

/// <summary>Validates the <see cref="DownloadProjectBootstrapPipelineCommand"/> before it is handled.</summary>
public sealed class DownloadProjectBootstrapPipelineCommandValidator : AbstractValidator<DownloadProjectBootstrapPipelineCommand>
{
    /// <summary>Initializes validation rules for downloading project bootstrap pipeline artifacts.</summary>
    public DownloadProjectBootstrapPipelineCommandValidator()
    {
        RuleFor(x => x.ProjectId.Value)
            .NotEmpty().WithMessage("ProjectId is required.");
    }
}
