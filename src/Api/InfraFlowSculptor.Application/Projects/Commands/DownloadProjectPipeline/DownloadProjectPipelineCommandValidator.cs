using FluentValidation;

namespace InfraFlowSculptor.Application.Projects.Commands.DownloadProjectPipeline;

/// <summary>Validates the <see cref="DownloadProjectPipelineCommand"/> before it is handled.</summary>
public sealed class DownloadProjectPipelineCommandValidator : AbstractValidator<DownloadProjectPipelineCommand>
{
    /// <summary>Initializes validation rules for downloading project pipeline artifacts.</summary>
    public DownloadProjectPipelineCommandValidator()
    {
        RuleFor(x => x.ProjectId.Value)
            .NotEmpty().WithMessage("ProjectId is required.");
    }
}